using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Logging;

namespace InternationalShopper.Bot.Services.Telegram.Commands;

public class TelegramUpdateService : ITelegramUpdateService
{
    private readonly Dictionary<string, ITelegramCommand> _commands;
    private readonly ILogger<TelegramUpdateService> _logger;

    public TelegramUpdateService(IEnumerable<ITelegramCommand> commands, ILogger<TelegramUpdateService> logger)
    {
        _commands = commands.ToDictionary(command => command.Name, StringComparer.OrdinalIgnoreCase);
        _logger = logger;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken = default)
    {
        var (commandName, message, callbackData) = GetCommand(update);

        if (commandName == null || message == null)
        {
            _logger.LogDebug("Ignoring update '{UpdateType}' because it does not contain a supported command.", update.Type);
            return;
        }

        if (!_commands.TryGetValue(commandName, out var command))
        {
            _logger.LogInformation("Command '{CommandName}' is not registered and will be skipped.", commandName);
            return;
        }

        try
        {
            var context = new TelegramCommandContext(update, message, callbackData);
            await command.ExecuteAsync(context, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute command '{CommandName}'.", commandName);
        }
    }

    private static (string? CommandName, Message? Message, string? CallbackData) GetCommand(Update update)
    {
        return update.Type switch
        {
            UpdateType.Message when update.Message is { } message => (TryGetCommandName(message), message, null),
            UpdateType.CallbackQuery when update.CallbackQuery is { } callbackQuery => TryGetCommandName(callbackQuery),
            _ => (null, null, null)
        };
    }

    private static (string? CommandName, Message? Message, string? CallbackData) TryGetCommandName(CallbackQuery callbackQuery)
    {
        if (string.IsNullOrWhiteSpace(callbackQuery.Data))
            return (null, callbackQuery.Message, null);

        var commandParts = callbackQuery.Data.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var commandName = commandParts.FirstOrDefault();
        var callbackData = commandParts.Length > 1 ? commandParts[1] : string.Empty;

        return (commandName, callbackQuery.Message, callbackData);
    }

    private static string? TryGetCommandName(Message message)
    {
        if (string.IsNullOrWhiteSpace(message.Text))
            return null;

        var commandEntity = message.Entities?.FirstOrDefault(entity => entity is { Offset: 0, Type: MessageEntityType.BotCommand });

        return commandEntity != null ? message.Text.Substring(commandEntity.Offset, commandEntity.Length) : null;
    }
}
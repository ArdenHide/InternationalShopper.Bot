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
        if (update is not { Type: UpdateType.Message, Message: { } message })
        {
            _logger.LogDebug("Ignoring update '{UpdateType}' because it is not a message.", update.Type);
            return;
        }

        var commandName = TryGetCommandName(message);
        if (commandName == null)
        {
            _logger.LogDebug("Received message without command: {Text}", message.Text);
            return;
        }

        if (!_commands.TryGetValue(commandName, out var command))
        {
            _logger.LogInformation("Command '{CommandName}' is not registered and will be skipped.", commandName);
            return;
        }

        try
        {
            var context = new TelegramCommandContext(update, message);
            await command.ExecuteAsync(context, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute command '{CommandName}'.", commandName);
        }
    }

    private static string? TryGetCommandName(Message message)
    {
        if (string.IsNullOrWhiteSpace(message.Text))
            return null;

        var commandEntity = message.Entities?.FirstOrDefault(entity => entity is { Offset: 0, Type: MessageEntityType.BotCommand });

        return commandEntity != null ? message.Text.Substring(commandEntity.Offset, commandEntity.Length) : null;
    }
}
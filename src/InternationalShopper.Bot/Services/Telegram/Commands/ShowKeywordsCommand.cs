using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.ReplyMarkups;
using InternationalShopper.Bot.Services.Reddit.Validators;
using InternationalShopper.Bot.Services.Telegram.Commands.Options;

namespace InternationalShopper.Bot.Services.Telegram.Commands;

public class ShowKeywordsCommand(ITelegramBotClient botClient, ILogger<ShowKeywordsCommand> logger) : ITelegramCommand
{
    public string Name => "/showkeywords";

    public async Task ExecuteAsync(TelegramCommandContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(context.CallbackData))
        {
            await SendEntitySelectionAsync(context.ChatId, cancellationToken);
            return;
        }

        await HandleCallbackAsync(context, cancellationToken);
    }

    private async Task SendEntitySelectionAsync(long chatId, CancellationToken cancellationToken)
    {
        var keyboard = new InlineKeyboardMarkup([[
            InlineKeyboardButton.WithCallbackData("Пост", BuildCallbackData(EntityOption.Post)),
            InlineKeyboardButton.WithCallbackData("Комментарий", BuildCallbackData(EntityOption.Comment))
        ]]);

        await botClient.SendMessage(chatId, "Какой тип показать?", replyMarkup: keyboard, cancellationToken: cancellationToken);
    }

    private async Task HandleCallbackAsync(TelegramCommandContext context, CancellationToken cancellationToken)
    {
        await AnswerCallbackQueryAsync(context.Update.CallbackQuery, cancellationToken);

        var callbackArguments = context.CallbackData!.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (callbackArguments.Length == 0)
        {
            logger.LogWarning("Received callback without arguments for command '{Command}'.", Name);
            return;
        }

        if (callbackArguments.Length == 1)
        {
            await SendListTypeSelectionAsync(context.ChatId, callbackArguments[0], cancellationToken);
            return;
        }

        await SendKeywordsAsync(context.ChatId, callbackArguments[0], callbackArguments[1], cancellationToken);
    }

    private async Task SendListTypeSelectionAsync(long chatId, string entityType, CancellationToken cancellationToken)
    {
        if (!TryParseOption<EntityOption>(entityType, out var entityOption))
        {
            logger.LogWarning("Unknown entity type '{EntityType}' in callback found while trying parse to '{NameOfEnumType}'.", entityType, nameof(EntityOption));
            return;
        }

        var keyboard = new InlineKeyboardMarkup([[
            InlineKeyboardButton.WithCallbackData("Белый список", BuildCallbackData(entityOption, ListOption.Whitelist)),
            InlineKeyboardButton.WithCallbackData("Чёрный список", BuildCallbackData(entityOption, ListOption.Blacklist))
        ]]);

        await botClient.SendMessage(chatId, "Какой список показать?", replyMarkup: keyboard, cancellationToken: cancellationToken);
    }

    private async Task SendKeywordsAsync(long chatId, string entityType, string listType, CancellationToken cancellationToken)
    {
        var entityOptionExists = TryParseOption<EntityOption>(entityType, out var entityOption);
        var options = entityOptionExists
            ? entityOption switch
            {
                EntityOption.Post => KeywordValidationOptions.Post,
                EntityOption.Comment => KeywordValidationOptions.Comment,
                _ => null
            }
            : null;

        if (options == null)
        {
            logger.LogWarning("Unknown entity type '{EntityType}' in callback found while trying parse to '{NameOfEnumType}'.", entityType, nameof(ListOption));
            return;
        }

        var listOptionExists = TryParseOption<ListOption>(listType, out var listOption);
        var keywords = listOptionExists
            ? listOption switch
            {
                ListOption.Whitelist => options.WhiteList,
                ListOption.Blacklist => options.BlackList,
                _ => null
            }
            : null;

        if (keywords == null)
        {
            logger.LogWarning("Unknown list type '{ListType}' in callback.", listType);
            return;
        }

        var listTitle = listOption == ListOption.Whitelist ? "Белый список" : "Чёрный список";
        var entityTitle = entityOption == EntityOption.Post ? "Поста" : "Комментария";
        var keywordsText = keywords.Count == 0
            ? "Список пуст."
            : string.Join('\n', keywords.Select(keyword => $"• {keyword}"));

        await botClient.SendMessage(chatId, $"{listTitle} для {entityTitle}:\n{keywordsText}", cancellationToken: cancellationToken);
    }

    private async Task AnswerCallbackQueryAsync(CallbackQuery? callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery == null)
            return;

        try
        {
            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to answer callback query {CallbackQueryId}.", callbackQuery.Id);
        }
    }

    private static bool TryParseOption<TOption>(string? value, out TOption option, params TOption[] allowedValues)
        where TOption : struct, Enum
    {
        option = default;

        if (!Enum.TryParse<TOption>(value, out var parsed))
            return false;

        if (!allowedValues.Contains(parsed))
            return Assign(default, out option, false);

        return Assign(parsed, out option, true);
    }

    private static bool Assign<T>(T value, out T option, bool result)
    {
        option = value;
        return result;
    }

    private string BuildCallbackData(EntityOption entityOption, ListOption? listOption = null)
    {
        var callbackBuilder = new StringBuilder()
            .Append(Name)
            .Append($" {entityOption.ToString()}");

        if (listOption != null)
            callbackBuilder.Append($" {listOption.ToString()}");

        return callbackBuilder.ToString();
    }
}
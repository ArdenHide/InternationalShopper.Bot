using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.ReplyMarkups;
using InternationalShopper.Bot.Services.Reddit.Validators;

namespace InternationalShopper.Bot.Services.Telegram.Commands;

public class ShowKeywordsCommand(ITelegramBotClient botClient, ILogger<ShowKeywordsCommand> logger) : ITelegramCommand
{
    private const string PostOption = "post";
    private const string CommentOption = "comment";
    private const string WhitelistOption = "whitelist";
    private const string BlacklistOption = "blacklist";

    public string Name => "/showkeywords";

    public async Task ExecuteAsync(TelegramCommandContext context, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(context.CallbackData))
        {
            await HandleCallbackAsync(context, cancellationToken);
            return;
        }

        await SendEntitySelectionAsync(context.ChatId, cancellationToken);
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

    private async Task SendEntitySelectionAsync(long chatId, CancellationToken cancellationToken)
    {
        var keyboard = new InlineKeyboardMarkup([[
            InlineKeyboardButton.WithCallbackData("Post", $"{Name} {PostOption}"),
            InlineKeyboardButton.WithCallbackData("Comment", $"{Name} {CommentOption}")
        ]]);

        await botClient.SendMessage(chatId, "Какой тип сущности показать?", replyMarkup: keyboard, cancellationToken: cancellationToken);
    }

    private async Task SendListTypeSelectionAsync(long chatId, string entityType, CancellationToken cancellationToken)
    {
        if (!IsEntityTypeSupported(entityType))
        {
            logger.LogWarning("Unknown entity type '{EntityType}' in callback.", entityType);
            return;
        }

        var keyboard = new InlineKeyboardMarkup([[
            InlineKeyboardButton.WithCallbackData("Белый список", $"{Name} {entityType} {WhitelistOption}"),
            InlineKeyboardButton.WithCallbackData("Чёрный список", $"{Name} {entityType} {BlacklistOption}")
        ]]);

        await botClient.SendMessage(chatId, "Какой список показать?", replyMarkup: keyboard, cancellationToken: cancellationToken);
    }

    private async Task SendKeywordsAsync(long chatId, string entityType, string listType, CancellationToken cancellationToken)
    {
        var options = entityType switch
        {
            PostOption => KeywordValidationOptions.Post,
            CommentOption => KeywordValidationOptions.Comment,
            _ => null
        };

        if (options == null)
        {
            logger.LogWarning("Unknown entity type '{EntityType}' in callback.", entityType);
            return;
        }

        var keywords = listType switch
        {
            WhitelistOption => options.WhiteList,
            BlacklistOption => options.BlackList,
            _ => null
        };

        if (keywords == null)
        {
            logger.LogWarning("Unknown list type '{ListType}' in callback.", listType);
            return;
        }

        var listTitle = listType == WhitelistOption ? "Белый список" : "Чёрный список";
        var entityTitle = entityType == PostOption ? "Поста" : "Комментария";
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

    private static bool IsEntityTypeSupported(string entityType) => entityType is PostOption or CommentOption;
}
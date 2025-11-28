using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Logging;

namespace InternationalShopper.Bot.Services.Telegram;

public class TelegramMessageService(ITelegramBotClient botClient, ILogger<TelegramMessageService> logger) : ITelegramMessageService
{
    public async Task SendMessageAsync(IEnumerable<long> chatsId, string message)
    {
        ArgumentNullException.ThrowIfNull(chatsId);
        ArgumentNullException.ThrowIfNull(message);

        var targets = chatsId.Where(chatId => chatId != 0).Distinct().ToArray();
        if (targets.Length == 0) return;

        var sendTasks = targets.Select(chatId => SendMessageAsync(chatId, message));
        await Task.WhenAll(sendTasks);
    }

    public async Task SendMessageAsync(long chatId, string message)
    {
        try
        {
            await botClient.SendMessage(chatId, message, parseMode: ParseMode.Html);
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to send message to chat {chatId}. Exception: {ex}", chatId, ex);
        }
    }
}
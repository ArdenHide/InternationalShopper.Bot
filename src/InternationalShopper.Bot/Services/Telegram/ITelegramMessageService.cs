namespace InternationalShopper.Bot.Services.Telegram;

public interface ITelegramMessageService
{
    public Task SendMessageAsync(IEnumerable<long> chatsId, string message);
    public Task SendMessageAsync(long chatId, string message);
}
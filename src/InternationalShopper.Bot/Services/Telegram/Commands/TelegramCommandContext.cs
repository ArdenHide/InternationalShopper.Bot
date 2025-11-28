using Telegram.Bot.Types;

namespace InternationalShopper.Bot.Services.Telegram.Commands;

public record TelegramCommandContext(Update Update, Message Message)
{
    public long ChatId => Message.Chat.Id;
    public long? UserId => Message.From?.Id;
    public string? UserName => Message.From?.Username;
    public string? Text => Message.Text;
}
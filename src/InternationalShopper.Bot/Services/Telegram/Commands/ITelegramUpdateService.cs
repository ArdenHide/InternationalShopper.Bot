using Telegram.Bot.Types;

namespace InternationalShopper.Bot.Services.Telegram.Commands;

public interface ITelegramUpdateService
{
    public Task HandleUpdateAsync(Update update, CancellationToken cancellationToken = default);
}
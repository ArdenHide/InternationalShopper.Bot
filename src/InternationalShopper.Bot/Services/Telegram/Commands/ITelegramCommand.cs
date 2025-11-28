namespace InternationalShopper.Bot.Services.Telegram.Commands;

public interface ITelegramCommand
{
    public string Name { get; }
    public Task ExecuteAsync(TelegramCommandContext context, CancellationToken cancellationToken = default);
}
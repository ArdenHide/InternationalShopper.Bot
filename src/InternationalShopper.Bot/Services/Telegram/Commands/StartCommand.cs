using Microsoft.Extensions.Logging;
using InternationalShopper.Database;
using InternationalShopper.Database.Models;

namespace InternationalShopper.Bot.Services.Telegram.Commands;

public class StartCommand(AppDbContext dbContext, ITelegramMessageService telegramMessageService, ILogger<StartCommand> logger) : ITelegramCommand
{
    public string Name => "/start";

    public async Task ExecuteAsync(TelegramCommandContext context, CancellationToken cancellationToken = default)
    {
        if (context.UserId == null)
        {
            logger.LogWarning("Cannot process /start because user information is missing in update '{UpdateId}'.", context.Update.Id);
            return;
        }

        var telegramUser = await dbContext.TelegramUsers.FindAsync([context.UserId.Value], cancellationToken);
        if (telegramUser == null)
        {
            dbContext.TelegramUsers.Add(new TelegramUser { Id = context.UserId.Value });
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("User '{UserName}' with ID '{UserId}' has been saved into DB.", context.UserName, context.UserId);

            await telegramMessageService.SendMessageAsync(context.ChatId, "You are successfully subscribed to notifications.");
            return;
        }

        logger.LogDebug("User '{UserId}' has already started the bot. Skipping registration.", context.UserId);
    }
}
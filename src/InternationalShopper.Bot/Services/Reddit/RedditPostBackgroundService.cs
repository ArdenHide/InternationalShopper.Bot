using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using InternationalShopper.Database;
using EnvironmentManager.Extensions;
using InternationalShopper.Database.Models;
using InternationalShopper.Bot.Services.Telegram;
using InternationalShopper.Bot.Services.Reddit.Http;
using InternationalShopper.Bot.Services.Reddit.Filters;
using InternationalShopper.Bot.Services.Reddit.Http.Models;

namespace InternationalShopper.Bot.Services.Reddit;

public class RedditPostBackgroundService(
    IRedditClient redditClient,
    IRedditPostFilter postFilter,
    ITelegramMessageService telegramMessageService,
    ILogger<RedditPostBackgroundService> logger,
    AppDbContext dbContext
) : BackgroundService
{
    private readonly int _delayMinutes = Env.REDDIT_SERVICE_DELAY_IN_MINUTES.GetRequired<int>();
    private readonly Random _random = new();

    private long _lastProcessedUtc;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitializeLastProcessedUtcAsync();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Running reddit post background service...");
            logger.LogInformation("Last processed time: {_lastProcessedUtc}", _lastProcessedUtc);
            try
            {
                logger.LogInformation("Downloading new posts data...");
                var newPosts = await GetNewPostsAsync();
                logger.LogInformation("Downloaded posts: {postsCount}.", newPosts.Count);
                if (newPosts.Count > 0)
                {
                    logger.LogInformation("Filtering new posts...");
                    await ProcessPostsAsync(newPosts);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle reddit posts");
            }

            var delay = GetRandomizedDelay();
            logger.LogInformation("Next reddit posts check scheduled after {delayMinutes} minutes.", delay.TotalMinutes);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private TimeSpan GetRandomizedDelay()
    {
        const double jitterPercentage = 0.2;
        var jitter = _random.NextDouble() * 2 * jitterPercentage - jitterPercentage;
        var minutesWithJitter = Math.Max(1, _delayMinutes * (1 + jitter));

        return TimeSpan.FromMinutes(minutesWithJitter);
    }

    private async Task<ICollection<PostData>> GetNewPostsAsync()
    {
        var posts = await redditClient.GetLatestPostsAsync();
        if (posts.Count == 0)
            return posts;

        var freshPosts = posts
            .Where(post => post.CreatedUTC > _lastProcessedUtc)
            .OrderBy(post => post.CreatedUTC)
            .ToList();

        if (freshPosts.Count > 0)
        {
            _lastProcessedUtc = freshPosts.Max(post => post.CreatedUTC);
            await UpdateLastProcessedUtcAsync(_lastProcessedUtc);
        }

        return freshPosts;
    }

    private async Task ProcessPostsAsync(ICollection<PostData> posts)
    {
        var filteredPosts = await postFilter.FilterAsync(posts);
        if (filteredPosts.Count == 0)
            return;

        logger.LogInformation("Founded posts after filter {filteredPostsCount}", filteredPosts.Count);

        foreach (var post in filteredPosts)
        {
            var message = post.ToString();
            var chatIds = await dbContext.TelegramUsers
                .AsNoTracking()
                .Select(x => x.Id)
                .ToArrayAsync();
            await telegramMessageService.SendMessageAsync(chatIds, message);
        }
    }

    private async Task InitializeLastProcessedUtcAsync()
    {
        var settings = await dbContext.BotSettings
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        _lastProcessedUtc = settings?.LastProcessedMessageUTC ?? 0;
    }

    private async Task UpdateLastProcessedUtcAsync(long lastProcessedUtc)
    {
        var settings = await dbContext.BotSettings.FirstOrDefaultAsync();

        if (settings is null)
        {
            settings = new BotSettings { LastProcessedMessageUTC = lastProcessedUtc };
            await dbContext.BotSettings.AddAsync(settings);
        }
        else
        {
            settings.LastProcessedMessageUTC = lastProcessedUtc;
        }

        await dbContext.SaveChangesAsync();
    }
}
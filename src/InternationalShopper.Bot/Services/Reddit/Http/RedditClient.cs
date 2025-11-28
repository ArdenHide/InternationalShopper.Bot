using Flurl.Http;
using System.Net;
using InternationalShopper.Bot.Services.Reddit.Http.Models;

namespace InternationalShopper.Bot.Services.Reddit.Http;

public sealed class RedditClient(IFlurlClient client) : IRedditClient
{
    private const string _subreddit = "internationalshopper";
    private const int _limit = 100;

    public async Task<ICollection<PostData>> GetLatestPostsAsync()
    {
        var request = client.Request("r", _subreddit, "new.json")
            .SetQueryParam("limit", _limit);

        var response = await SendWithRetriesAsync(request);
        if (response.StatusCode == (int)HttpStatusCode.NotModified)
            return Array.Empty<PostData>();

        var posts = await response.GetJsonAsync<HttpRedditResponse<PostData>>();
        if (posts?.Data.Children is { Count: > 0 } children)
            return children.Select(c => c.Data).ToList();

        return Array.Empty<PostData>();
    }

    public async Task<ICollection<PostComment>> GetPostCommentsAsync(string permalink)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permalink);

        var request = client.Request(permalink, ".json");

        var response = await SendWithRetriesAsync(request);
        var listings = await response.GetJsonAsync<List<HttpRedditResponse<CommentData>>>();

        if (listings?.Count > 1)
        {
            var commentsRoot = listings[1];
            var collectedComments = new List<CommentData>();
            if (commentsRoot.Data.Children is { Count: > 0 } children)
            {
                foreach (var child in children)
                {
                    if (child.Data is { } comment)
                        CollectComments(comment, collectedComments);
                }
            }

            return collectedComments.Select(x => new PostComment(x)).ToArray();
        }

        return [];
    }

    private static void CollectComments(CommentData comment, ICollection<CommentData> accumulator)
    {
        accumulator.Add(comment);

        if (comment.Replies?.Data.Children is not { Count: > 0 } replies) return;

        foreach (var child in replies)
        {
            if (child.Data is { } reply)
                CollectComments(reply, accumulator);
        }
    }

    private static async Task<IFlurlResponse> SendWithRetriesAsync(IFlurlRequest req)
    {
        const int maxAttempts = 4;
        var delay = TimeSpan.FromSeconds(1);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await req.GetAsync();
            }
            catch (FlurlHttpException ex) when (ShouldRetry(ex))
            {
                var retryAfter = GetRetryAfter(ex);
                if (retryAfter != null)
                {
                    await Task.Delay(retryAfter.Value);
                }
                else
                {
                    var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(250, 750));
                    await Task.Delay(delay + jitter);
                    delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
                }

                if (attempt == maxAttempts) throw;
            }
        }

        throw new InvalidOperationException("Unreachable Reddit service.");
    }

    private static bool ShouldRetry(FlurlHttpException ex)
    {
        var code = ex.Call?.Response?.StatusCode ?? 0;
        return code is 429 or >= 500;
    }

    private static TimeSpan? GetRetryAfter(FlurlHttpException ex)
    {
        var header = ex.Call?.Response?.Headers?
            .FirstOrDefault(h => string.Equals(h.Name, "Retry-After", StringComparison.OrdinalIgnoreCase))
            .Value;

        if (header == null) return null;

        if (int.TryParse(header, out var seconds) && seconds >= 0)
            return TimeSpan.FromSeconds(seconds);

        return null;
    }
}
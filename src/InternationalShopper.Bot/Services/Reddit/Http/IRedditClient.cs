using InternationalShopper.Bot.Services.Reddit.Http.Models;

namespace InternationalShopper.Bot.Services.Reddit.Http;

public interface IRedditClient
{
    public Task<ICollection<PostData>> GetLatestPostsAsync();
    public Task<ICollection<PostComment>> GetPostCommentsAsync(string permalink);
}
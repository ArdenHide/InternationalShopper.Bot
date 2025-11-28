using InternationalShopper.Bot.Services.Reddit.Http.Models;

namespace InternationalShopper.Bot.Services.Reddit.Filters;

public interface IRedditPostFilter
{
    public Task<ICollection<PostData>> FilterAsync(ICollection<PostData> posts);
}
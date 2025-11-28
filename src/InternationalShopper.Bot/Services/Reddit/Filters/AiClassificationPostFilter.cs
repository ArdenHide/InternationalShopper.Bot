using InternationalShopper.Bot.Services.Reddit.Http.Models;

namespace InternationalShopper.Bot.Services.Reddit.Filters;

public class AiClassificationPostFilter : IRedditPostFilter
{
    public Task<ICollection<PostData>> FilterAsync(ICollection<PostData> posts)
    {
        throw new NotImplementedException();
    }
}
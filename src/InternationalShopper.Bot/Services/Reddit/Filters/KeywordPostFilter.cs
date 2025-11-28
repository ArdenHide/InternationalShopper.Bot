using FluentValidation;
using InternationalShopper.Bot.Services.Reddit.Http.Models;

namespace InternationalShopper.Bot.Services.Reddit.Filters;

public class KeywordPostFilter(IValidator<PostData> postValidator) : IRedditPostFilter
{
    public async Task<ICollection<PostData>> FilterAsync(ICollection<PostData> posts)
    {
        var result = new List<PostData>(posts.Count);

        foreach (var post in posts)
        {
            var validationResult = await postValidator.ValidateAsync(post);

            if (validationResult.IsValid)
                result.Add(post);
        }

        return result;
    }
}
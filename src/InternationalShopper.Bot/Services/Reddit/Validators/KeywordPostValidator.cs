using FluentValidation;
using InternationalShopper.Bot.Services.Reddit.Http;
using InternationalShopper.Bot.Services.Reddit.Http.Models;

namespace InternationalShopper.Bot.Services.Reddit.Validators;

public class KeywordPostValidator : AbstractValidator<PostData>
{
    private readonly IRedditClient _redditClient;
    private readonly IValidator<PostComment> _commentValidator;

    public KeywordPostValidator(IRedditClient redditClient, IValidator<PostComment> commentValidator)
    {
        _redditClient = redditClient;
        _commentValidator = commentValidator;

        ClassLevelCascadeMode = CascadeMode.Stop;
        
        RuleFor(post => post)
            .NotNull()
            .NotEmpty();

        RuleFor(post => post.Title)
            .SetValidator(new KeywordStringValidator(KeywordValidationOptions.Post));

        RuleFor(post => post.Text)
            .SetValidator(new KeywordStringValidator(KeywordValidationOptions.Post));

        RuleFor(post => post)
            .MustAsync(CommentsAreValidAsync);
    }

    private async Task<bool> CommentsAreValidAsync(PostData post, CancellationToken _)
    {
        if (post.CommentsCount <= 0)
            return true;

        var comments = await _redditClient.GetPostCommentsAsync(post.Permalink);

        return comments.All(comment => _commentValidator.Validate(comment).IsValid);
    }
}

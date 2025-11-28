using FluentValidation;
using InternationalShopper.Bot.Services.Reddit.Http.Models;

namespace InternationalShopper.Bot.Services.Reddit.Validators;

public class KeywordCommentValidator : AbstractValidator<PostComment>
{
    public KeywordCommentValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(post => post.Author)
            .SetValidator(new KeywordStringValidator(KeywordValidationOptions.Comment))
            .When(comment => !string.IsNullOrWhiteSpace(comment.Author));

        RuleFor(post => post.Body)
            .SetValidator(new KeywordStringValidator(KeywordValidationOptions.Comment))
            .When(comment => !string.IsNullOrWhiteSpace(comment.Body));

        RuleFor(post => post.FlairText)
            .SetValidator(new KeywordStringValidator(KeywordValidationOptions.Comment))
            .When(comment => !string.IsNullOrWhiteSpace(comment.FlairText));
    }
}
using FluentValidation;

namespace InternationalShopper.Bot.Services.Reddit.Validators;

public class KeywordStringValidator : AbstractValidator<string?>
{
    private readonly KeywordValidationOptions _options;

    public KeywordStringValidator(KeywordValidationOptions options)
    {
        _options = options;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(text => text)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(text => text)
            .Must(NotContainsBlacklistedKeyword!)
            .When(_ => _options.BlackList != null);

        RuleFor(text => text)
            .Must(ContainsWhitelistedKeyword!)
            .When(_ => _options.WhiteList != null);
    }

    private bool ContainsWhitelistedKeyword(string text) =>
        _options.WhiteList?.Any(x => text.Contains(x, StringComparison.OrdinalIgnoreCase)) ?? true;

    private bool NotContainsBlacklistedKeyword(string text) =>
        !_options.BlackList?.Any(x => text.Contains(x, StringComparison.OrdinalIgnoreCase)) ?? true;
}
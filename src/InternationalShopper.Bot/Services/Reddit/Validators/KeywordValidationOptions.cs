using EnvironmentManager.Extensions;

namespace InternationalShopper.Bot.Services.Reddit.Validators;

public class KeywordValidationOptions(IEnumerable<string>? whitelist, IEnumerable<string>? blacklist)
{
    public IReadOnlyCollection<string>? WhiteList { get; } = whitelist?.ToArray();
    public IReadOnlyCollection<string>? BlackList { get; } = blacklist?.ToArray();

    private static IReadOnlyCollection<string> PostWhitelist { get; } = ["Vinted", "Germany", "Kleinanzeigen"];
    private static IReadOnlyCollection<string> PostBlacklist { get; } = ["to Germany", "Vinted UK", "from UK", "Vinted Poland"];
    private static IReadOnlyCollection<string>? CommentWhitelist => null;
    private static IReadOnlyCollection<string> CommentBlacklist { get; } = [Env.REDDIT_USER_NAME.GetRequired()];

    public static KeywordValidationOptions Post { get; } = new(
        whitelist: PostWhitelist,
        blacklist: PostBlacklist
    );

    public static KeywordValidationOptions Comment { get; } = new(
        whitelist: CommentWhitelist,
        blacklist: CommentBlacklist
    );
}
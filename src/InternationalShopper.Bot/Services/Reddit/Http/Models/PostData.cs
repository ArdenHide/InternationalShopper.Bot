using System.Text;
using Newtonsoft.Json;

namespace InternationalShopper.Bot.Services.Reddit.Http.Models;

[method: JsonConstructor]
public record PostData(
    [JsonProperty("title")] string Title,
    [JsonProperty("selftext")] string Text,
    [JsonProperty("url")] string? Url,
    [JsonProperty("permalink")] string Permalink,
    [JsonProperty("num_comments")] int CommentsCount,
    [JsonProperty("created_utc")] long CreatedUTC
) : IData
{
    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.AppendLine($"<b>{Title}</b>");

        if (!string.IsNullOrWhiteSpace(Text))
        {
            builder
                .AppendLine()
                .AppendLine(TrimText(Text));
        }

        var created = DateTimeOffset.FromUnixTimeSeconds(CreatedUTC).UtcDateTime;
        var berlinZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var berlinTime = TimeZoneInfo.ConvertTime(created, berlinZone);
        var commentsWord = GetCommentsWordRu(CommentsCount);

        builder.AppendLine();
        builder.AppendLine($"💬 Уже {commentsWord}: {CommentsCount} | 🕒 Опубликовано в: {berlinTime:dd.MM.yyyy HH:mm} ");
        builder.AppendLine($"➡️ <a href=\"https://reddit.com{Permalink}\">Открыть ссылку ✨</a>");

        return builder.ToString();
    }

    private static string GetCommentsWordRu(int count)
    {
        var n = count % 100;
        if (n is >= 11 and <= 19) return "комментариев";

        n %= 10;
        return n switch
        {
            1 => "комментарий",
            2 or 3 or 4 => "комментария",
            _ => "комментариев"
        };
    }

    private static string TrimText(string text, int maxLength = 100)
    {
        var preview = text.Trim();

        if (preview.Length > maxLength)
            return preview[..maxLength] + "…";

        return preview;
    }
}
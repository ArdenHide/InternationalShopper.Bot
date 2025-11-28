using Newtonsoft.Json;

namespace InternationalShopper.Bot.Services.Reddit.Http.Models;

[method: JsonConstructor]
public record CommentData(
    [JsonProperty("body")] string Body,
    [JsonProperty("author_flair_text")] string? FlairText,
    [JsonProperty("author")] string Author,
    [JsonProperty("replies")] HttpRedditResponse<CommentData>? Replies
) : IData;
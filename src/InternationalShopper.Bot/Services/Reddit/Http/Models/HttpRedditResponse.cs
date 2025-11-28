using Newtonsoft.Json;

namespace InternationalShopper.Bot.Services.Reddit.Http.Models;

[method: JsonConstructor]
public record HttpRedditResponse<TData>(
    [JsonProperty("kind")] string Kind,
    [JsonProperty("data")] Data<TData> Data
) where TData : IData;
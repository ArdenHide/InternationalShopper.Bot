using Newtonsoft.Json;

namespace InternationalShopper.Bot.Services.Reddit.Http.Models;

[method: JsonConstructor]
public record Children<TData>(
    [JsonProperty("kind")] string Kind,
    [JsonProperty("data")] TData Data
) where TData : IData;
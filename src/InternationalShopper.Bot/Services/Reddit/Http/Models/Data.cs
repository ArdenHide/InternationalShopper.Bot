using Newtonsoft.Json;

namespace InternationalShopper.Bot.Services.Reddit.Http.Models;

[method: JsonConstructor]
public record Data<TData>(
    [JsonProperty("after")] string? After,
    [JsonProperty("dist")] int? Dist,
    [JsonProperty("modhash")] string? ModHash,
    [JsonProperty("geo_filter")] string? GeoFilter,
    [JsonProperty("children")] List<Children<TData>>? Children,
    [JsonProperty("before")] string? Before
) where TData : IData;
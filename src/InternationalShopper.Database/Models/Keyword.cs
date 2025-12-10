using InternationalShopper.Database.Models.Types;

namespace InternationalShopper.Database.Models;

public class Keyword
{
    public long Id { get; set; }

    public string Text { get; set; } = null!;

    public KeywordListType ListType { get; set; }
    public KeywordEntityType EntityType { get; set; }

    public long TrackedCountryId { get; set; }
    public TrackedCountry TrackedCountry { get; set; } = null!;
}
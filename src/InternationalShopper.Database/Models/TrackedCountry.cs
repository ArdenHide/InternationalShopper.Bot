using InternationalShopper.Database.Models.Types;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternationalShopper.Database.Models;

public class TrackedCountry
{
    public long Id { get; set; }

    public long TelegramUserId { get; set; }
    public TelegramUser TelegramUser { get; set; } = null!;

    public string Name { get; set; } = null!;

    public List<Keyword> Keywords { get; set; } = [];

    [NotMapped]
    public IEnumerable<Keyword> PostWhitelist => Keywords.Where(k => k is { ListType: KeywordListType.WhiteList, EntityType: KeywordEntityType.Post });

    [NotMapped]
    public IEnumerable<Keyword> PostBlacklist => Keywords.Where(k => k is { ListType: KeywordListType.BlackList, EntityType: KeywordEntityType.Post });

    [NotMapped]
    public IEnumerable<Keyword> CommentWhitelist => Keywords.Where(k => k is { ListType: KeywordListType.WhiteList, EntityType: KeywordEntityType.Comment });

    [NotMapped]
    public IEnumerable<Keyword>  CommentBlacklist => Keywords.Where(k => k is { ListType: KeywordListType.BlackList, EntityType: KeywordEntityType.Comment });
}
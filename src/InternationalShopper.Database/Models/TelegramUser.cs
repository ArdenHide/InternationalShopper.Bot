namespace InternationalShopper.Database.Models;

public class TelegramUser
{
    public long Id { get; set; }

    public List<TrackedCountry> TrackedCountries { get; set; } = [];
}
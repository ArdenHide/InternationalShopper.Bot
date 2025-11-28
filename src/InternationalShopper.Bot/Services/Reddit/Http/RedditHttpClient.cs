using Flurl.Http;
using Flurl.Http.Newtonsoft;

namespace InternationalShopper.Bot.Services.Reddit.Http;

public class RedditFlurlClient : FlurlClient
{
    public const string RedditUrl = "https://www.reddit.com/";

    public RedditFlurlClient() : base(RedditUrl)
    {
        Headers.Add("Accept", "*");
        Headers.Add("User-Agent", "InternationalShopper/1.0.0");
        Settings.JsonSerializer = new NewtonsoftJsonSerializer();
    }
}
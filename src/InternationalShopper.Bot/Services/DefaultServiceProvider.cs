using Flurl.Http;
using Telegram.Bot;
using FluentValidation;
using Telegram.Bot.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using EnvironmentManager.Extensions;
using InternationalShopper.Bot.Extensions;
using Microsoft.Extensions.DependencyInjection;
using InternationalShopper.Bot.Services.Reddit;
using InternationalShopper.Bot.Services.Telegram;
using InternationalShopper.Bot.Services.Reddit.Http;
using InternationalShopper.Bot.Services.Reddit.Filters;
using InternationalShopper.Bot.Services.Telegram.Commands;
using InternationalShopper.Bot.Services.Reddit.Validators;
using InternationalShopper.Bot.Services.Reddit.Http.Models;

namespace InternationalShopper.Bot.Services;

public static class DefaultServiceProvider
{
    public static WebApplication BuildWebApplication(string[] args)
    {
#if DEBUG
        DotNetEnv.Env.TraversePath().Load();
#endif

        var builder = WebApplication.CreateBuilder(args);

        ConfigureWebHost(builder);
        var webhookSecret = ConfigureServices(builder.Services);

        var app = builder.Build();
        MapTelegramWebhook(app, webhookSecret);

        return app;
    }

    private static void ConfigureWebHost(WebApplicationBuilder builder)
    {
        builder.WebHost.UseUrls(Env.ASPNETCORE_URLS.GetRequired());
    }

    private static string ConfigureServices(IServiceCollection services)
    {
        var botToken = Env.TELEGRAM_TOKEN.GetRequired();
        var webhookSecret = Env.TELEGRAM_WEBHOOK_SECRET.GetRequired();

        services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(botToken));
        services.AddSingleton<IFlurlClient, RedditFlurlClient>();
        services.AddSingleton<IRedditClient, RedditClient>();
        services.AddSingleton<IValidator<PostComment>, KeywordCommentValidator>();
        services.AddSingleton<IValidator<PostData>, KeywordPostValidator>();
        services.AddSingleton<IRedditPostFilter, KeywordPostFilter>();
        services.AddSingleton<ITelegramMessageService, TelegramMessageService>();
        services.AddScoped<ITelegramUpdateService, TelegramUpdateService>();
        services.AddScoped<ITelegramCommand, StartCommand>();
        services.AddScoped<ITelegramCommand, ShowKeywordsCommand>();
#if DEBUG
        services.AddSqliteDbContext();
#else
        services.AddNpgsqlDbContext();
#endif
        services.AddHostedService<RedditPostBackgroundService>();

        return webhookSecret;
    }

    private static void MapTelegramWebhook(WebApplication app, string webhookSecret)
    {
        app.MapPost("/telegram/webhook", async (HttpRequest req, Update update, ITelegramUpdateService updateService, ILogger<Program> log) =>
        {
            if (!req.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var got) || got != webhookSecret)
                return Results.Unauthorized();

            try
            {
                await updateService.HandleUpdateAsync(update);
                return Results.Ok();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Webhook handling failed.");
                return Results.Ok();
            }
        });
    }
}
using Npgsql;
using EnvironmentManager.Extensions;
using InternationalShopper.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InternationalShopper.Bot.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNpgsqlDbContext(this IServiceCollection collection) =>
        collection.AddDbContext<AppDbContext>(options => options.UseNpgsql(new NpgsqlConnectionStringBuilder
        {
            Host = Env.DATABASE_HOST.GetRequired(),
            Database = Env.DATABASE_NAME.GetRequired(),
            Username = Env.DATABASE_USER.GetRequired(),
            Password = Env.DATABASE_PASSWORD.GetRequired()
        }.ConnectionString));

    public static IServiceCollection AddSqliteDbContext(this IServiceCollection collection) =>
        collection.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=internationalshopper.db"));
}
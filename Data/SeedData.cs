using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Driver;
using RbacApi.Data.Entities;

namespace RbacApi.Data;

public static class SeedData
{
    public static void Execute(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<QuotesDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        InitPopulationDB(context, configuration);
    }

    private static void InitPopulationDB(QuotesDbContext context, IConfiguration configuration)
    {
        string seedDataPath = configuration["Paths:SeedData"] ?? "C:\\rbac\\seed";

        // Permissions
        if (!context.Permissions.Any())
        {
            Console.WriteLine("--> Seeding permissions...");
            var docs = LoadFromJson<Permission>(Path.Combine(seedDataPath, $"{CollectionNames.Permissions}.json"));
            context.Permissions.AddRange(docs);
            context.SaveChanges();
        }

        // Roles
        if (!context.Roles.Any())
        {
            Console.WriteLine("--> Seeding roles...");
            var docs = LoadFromJson<Role>(Path.Combine(seedDataPath, $"{CollectionNames.Roles}.json"));
            context.Roles.AddRange(docs);
            context.SaveChanges();
        }

        // Menu items
        if (!context.MenuItems.Any())
        {
            Console.WriteLine("--> Seeding menu items...");
            var docs = LoadFromJson<MenuItem>(Path.Combine(seedDataPath, $"{CollectionNames.Menuitems}.json"));
            context.MenuItems.AddRange(docs);
            context.SaveChanges();
        }
    }

    private static IEnumerable<T> LoadFromJson<T>(string filename)
    {
        var options = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        options.Converters.Add(new JsonStringEnumConverter());

        var json = File.ReadAllText(filename);
        var docs = JsonSerializer.Deserialize<IEnumerable<T>>(json, options);
        return docs ?? [];
    }
}
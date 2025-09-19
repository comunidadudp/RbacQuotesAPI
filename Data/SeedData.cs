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

        var collections = scope.ServiceProvider.GetRequiredService<CollectionsProvider>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        InitPopulationDB(collections, configuration);
    }

    private static void InitPopulationDB(CollectionsProvider collections, IConfiguration configuration)
    {
        string seedDataPath = configuration["Paths:SeedData"] ?? "C:\\rbac\\seed";

        // Permissions
        if (collections.Permissions.CountDocuments(FilterDefinition<Permission>.Empty) == 0)
        {
            Console.WriteLine("--> Seeding permissions...");
            var docs = LoadFromJson<Permission>(Path.Combine(seedDataPath, $"{CollectionNames.Permissions}.json"));
            collections.Permissions.InsertMany(docs);
        }

        // Roles
        if (collections.Roles.CountDocuments(FilterDefinition<Role>.Empty) == 0)
        {
            Console.WriteLine("--> Seeding roles...");
            var docs = LoadFromJson<Role>(Path.Combine(seedDataPath, $"{CollectionNames.Roles}.json"));
            collections.Roles.InsertMany(docs);
        }

        // Menu items
        if (collections.MenuItems.CountDocuments(FilterDefinition<MenuItem>.Empty) == 0)
        {
            Console.WriteLine("--> Seeding menu items...");
            var docs = LoadFromJson<MenuItem>(Path.Combine(seedDataPath, $"{CollectionNames.Menuitems}.json"));
            collections.MenuItems.InsertMany(docs);
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
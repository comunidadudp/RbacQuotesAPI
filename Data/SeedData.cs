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

        var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        InitPopulationDB(database, configuration);
    }

    private static void InitPopulationDB(IMongoDatabase database, IConfiguration configuration)
    {
        string seedDataPath = configuration["Paths:SeedData"] ?? "C:\\rbac\\seed";

        // Permissions
        var permissions = database.GetCollection<Permission>("permissions");
        if (permissions.CountDocuments(FilterDefinition<Permission>.Empty) == 0)
        {
            Console.WriteLine("--> Seeding permissions...");
            var docs = LoadFromJson<Permission>(Path.Combine(seedDataPath, "permissions.json"));
            permissions.InsertMany(docs);
        }

        // Roles
        var roles = database.GetCollection<Role>("roles");
        if (roles.CountDocuments(FilterDefinition<Role>.Empty) == 0)
        {
            Console.WriteLine("--> Seeding roles...");
            var docs = LoadFromJson<Role>(Path.Combine(seedDataPath, "roles.json"));
            roles.InsertMany(docs);
        }

        // Menu items
        var menuItems = database.GetCollection<MenuItem>("menu_items");
        if (menuItems.CountDocuments(FilterDefinition<MenuItem>.Empty) == 0)
        {
            Console.WriteLine("--> Seeding menu items...");
            var docs = LoadFromJson<MenuItem>(Path.Combine(seedDataPath, "menu_items.json"));
            menuItems.InsertMany(docs);
        }
    }

    private static IEnumerable<T>? LoadFromJson<T>(string filename)
    {
        var options = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        options.Converters.Add(new JsonStringEnumConverter());

        var json = File.ReadAllText(filename);
        var docs = JsonSerializer.Deserialize<IEnumerable<T>>(json, options);
        return docs;
    }
}
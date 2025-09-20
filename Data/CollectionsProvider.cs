using MongoDB.Driver;
using RbacApi.Data.Entities;

namespace RbacApi.Data;

public class CollectionsProvider(IMongoDatabase database)
{

   public IMongoCollection<AuditLog> AuditLogs
      => database.GetCollection<AuditLog>(CollectionNames.AuditLogs);
      
    public IMongoCollection<MenuItem> MenuItems
     => database.GetCollection<MenuItem>(CollectionNames.Menuitems);

    public IMongoCollection<Permission> Permissions
       => database.GetCollection<Permission>(CollectionNames.Permissions);

    public IMongoCollection<RefreshToken> RefreshTokens
       => database.GetCollection<RefreshToken>(CollectionNames.RefreshTokens);

    public IMongoCollection<Role> Roles
       => database.GetCollection<Role>(CollectionNames.Roles);

    public IMongoCollection<User> Users
       => database.GetCollection<User>(CollectionNames.Users);
}
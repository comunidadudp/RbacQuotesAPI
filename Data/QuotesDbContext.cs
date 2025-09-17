using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using RbacApi.Data.Entities;

namespace RbacApi.Data;

public class QuotesDbContext(DbContextOptions<QuotesDbContext> options) : DbContext(options)
{
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<MenuItem>().ToCollection(CollectionNames.Menuitems);
        modelBuilder.Entity<Permission>().ToCollection(CollectionNames.Permissions);
        modelBuilder.Entity<Role>().ToCollection(CollectionNames.Roles);
        modelBuilder.Entity<User>().ToCollection(CollectionNames.Users);
    }
}

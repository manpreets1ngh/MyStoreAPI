using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Reflection.Emit;
using MyStoreAPI.Areas.Identity.Data;
using MyStoreAPI.Models;

namespace MyStoreAPI.Data;

public class MyStoreAPIIdentityContext : IdentityDbContext<MyStoreAPIUser>
{
    public MyStoreAPIIdentityContext(DbContextOptions<MyStoreAPIIdentityContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Fix SQL Server-specific types for PostgreSQL
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var props = entity.ClrType.GetProperties();
            foreach (var prop in props)
            {
                if (prop.PropertyType == typeof(Guid) || prop.PropertyType == typeof(Guid?))
                    builder.Entity(entity.Name).Property(prop.Name).HasColumnType("uuid");

                if (prop.PropertyType == typeof(string))
                    builder.Entity(entity.Name).Property(prop.Name).HasColumnType("text");
            }
        }

        base.OnModelCreating(builder);
    }

    public DbSet<AddressModel> Addresses { get; set; }
    public DbSet<CardModel> CardDetails { get; set; }

}

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
        base.OnModelCreating(builder);
/*
        builder.Entity<AddressModel>()
        .HasOne<MyStoreAPIUser>(a => a.User)
        .WithMany(u => u.Addresses)
        .HasForeignKey(a => a.UserId);*/
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }

    public DbSet<AddressModel> Addresses { get; set; }
    public DbSet<CardModel> CardDetails { get; set; }

}

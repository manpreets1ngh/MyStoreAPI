using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyStoreAPI.Models;

namespace MyStoreAPI.Data
{
    public class MyStoreAPIContext : DbContext
    {
        public MyStoreAPIContext (DbContextOptions<MyStoreAPIContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = default!;
        public DbSet<Order> Orders { get; set; } = default!;
        public DbSet<ShippingInfoModel> ShippingInfos { get; set; } = default!;
        public DbSet<StatusModel> Status { get; set; } = default!;
        public DbSet<OrderDetail> OrderDetails { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Fix SQL Server-specific types for PostgreSQL
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var props = entity.ClrType.GetProperties();
                foreach (var prop in props)
                {
                    if (prop.PropertyType == typeof(Guid) || prop.PropertyType == typeof(Guid?))
                        modelBuilder.Entity(entity.Name).Property(prop.Name).HasColumnType("uuid");

                    if (prop.PropertyType == typeof(string))
                        modelBuilder.Entity(entity.Name).Property(prop.Name).HasColumnType("text");
                }
            }
            
            base.OnModelCreating(modelBuilder);

            // Explicit table name mapping
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OrderDetail>().ToTable("OrderDetails");
            modelBuilder.Entity<ShippingInfoModel>().ToTable("ShippingInfos");
            modelBuilder.Entity<StatusModel>().ToTable("Status");

            // Unique index for OrderNumber in Orders
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            // Relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.ShippingInfo)
                .WithMany()
                .HasForeignKey(o => o.ShippingInfoId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.ShippingAddress)
                .WithMany()
                .HasForeignKey(o => o.ShippingAddressId);

            modelBuilder.Entity<ShippingInfoModel>()
                .HasOne(s => s.DeliveryStatus)
                .WithMany()
                .HasForeignKey(s => s.StatusId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany()
                .HasForeignKey(od => od.ProductId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Address)
                .WithMany()
                .HasForeignKey(od => od.AddressId);

        }

    }
}

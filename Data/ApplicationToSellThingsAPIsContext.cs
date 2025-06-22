using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ApplicationToSellThings.APIs.Models;

namespace ApplicationToSellThings.APIs.Data
{
    public class ApplicationToSellThingsAPIsContext : DbContext
    {
        public ApplicationToSellThingsAPIsContext (DbContextOptions<ApplicationToSellThingsAPIsContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationToSellThings.APIs.Models.Product> Products { get; set; } = default!;
        public DbSet<ApplicationToSellThings.APIs.Models.Order> Orders { get; set; } = default!;
        public DbSet<ApplicationToSellThings.APIs.Models.ShippingInfoModel> ShippingInfos { get; set; } = default!;
        public DbSet<ApplicationToSellThings.APIs.Models.StatusModel> Status { get; set; } = default!;
        public DbSet<ApplicationToSellThings.APIs.Models.OrderDetail> OrderDetails { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

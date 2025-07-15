using System.ComponentModel.DataAnnotations;

namespace ApplicationToSellThings.APIs.Models
{
    public class Product
    {
        [Key]
        public Guid ProductId { get; set; }
        public string BrandName { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public decimal? Discount { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int QuantityInStock { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string ProductImage { get; set; }  // New property for storing image data

    }
}

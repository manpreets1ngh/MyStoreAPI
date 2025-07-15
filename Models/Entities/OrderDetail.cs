using System.ComponentModel.DataAnnotations;

namespace ApplicationToSellThings.APIs.Models
{
    public class OrderDetail
    {
        [Key]
        public Guid OrderDetailId { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public Guid AddressId { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
        public Product Product { get; set; }
        public AddressResponseViewModel Address { get; set; }

    }
}

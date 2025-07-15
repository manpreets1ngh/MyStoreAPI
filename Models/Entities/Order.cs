using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MyStoreAPI.Models
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(20)] // Adjust length as needed
        public string OrderNumber { get; set; }

        public string PaymentMethod { get; set; }
        public Guid? CardId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Tax { get; set; }
        public string OrderStatus { get; set; }
        public DateTime? OrderCreatedAt { get; set; }
        public DateTime? OrderUpdatedAt { get; set; }

        public Guid ShippingInfoId {  get; set; }
        public ShippingInfoModel ShippingInfo { get; set; }

        public Guid ShippingAddressId { get; set; }
        public AddressModel ShippingAddress { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
        public string? ShippingMethod { get; set; }
    }
}

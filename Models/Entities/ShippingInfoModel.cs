using System.ComponentModel.DataAnnotations;

namespace ApplicationToSellThings.APIs.Models
{
    public class ShippingInfoModel
    {
        [Key]
        public Guid ShippingInfoId { get; set; } 
        public DateTime? ShippingDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public int StatusId { get; set; }
        public StatusModel DeliveryStatus { get; set; }
    }
}

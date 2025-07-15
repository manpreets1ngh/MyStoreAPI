namespace MyStoreAPI.Models
{
    public class OrderApiResponseModel
    {
        public string OrderNumber { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Tax { get; set; }
        public string OrderStatus { get; set; }
        public int Quantity { get; set; }
        public List<OrderDetailApiResponseModel> OrderDetails { get; set; }
        public DateTime? OrderCreatedAt { get; set; }
public string? ShippingMethod { get; set; }
    }
}

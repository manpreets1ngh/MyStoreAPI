namespace MyStoreAPI.Models
{
    public class OrderApiRequestModel
    {
        public Guid UserId { get; set; }
        public List<OrderProductApiModel> Products { get; set; }
        public Guid? CardId { get; set; }
        public Guid ShippingAddressId { get; set; }
        public string PaymentMethod { get; set; }
        public int Quantity { get; set; }
        public string? ShippingMethod { get; set; }
    }
}

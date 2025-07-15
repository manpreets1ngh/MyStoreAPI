namespace MyStoreAPI.Models
{
    public class UpdateOrderStatusAndShippingInfoRequest
    {
        public string OrderStatus { get; set; }
        public ShippingInfoModel? ShippingInfo { get; set; }
    }
}

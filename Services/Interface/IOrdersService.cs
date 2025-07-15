using MyStoreAPI.Models;

namespace MyStoreAPI.Services.Interface
{
    public interface IOrdersService
    {
        Task<ResponseModel<OrderApiResponseModel>> CreateOrder(OrderApiRequestModel orderRequestModel);
        Task<ResponseModel<Order>> GetOrdersByUserId(Guid userId);
        Task<ResponseModel<Order>> GetOrderByOrderNumber(string orderNumber);
        Task<ResponseModel<Order>> GetAllOrdersAsync();
        Task<ResponseModel<Order>> UpdateOrderById(Guid orderId, Order orderModel);
        Task<ResponseModel<ShippingInfoModel>> GetShippingInfo(Guid orderId);
        Task<ResponseModel<ShippingInfoModel>> UpdateShippingInfo(Guid orderId, ShippingInfoModel shippingInfoModel);
        Task<ResponseModel<Order>> UpdateOrderStatusAndShippingInfo(Guid orderId, string newStatus, ShippingInfoModel? shippingInfoUpdates = null);
    }
}

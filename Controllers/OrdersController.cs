using MyStoreAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStoreAPI.Models;
using MyStoreAPI.Services.Interface;

namespace MyStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _orderService;
        private readonly IConfiguration _config;
        public OrdersController(IOrdersService orderService, IConfiguration config)
        {
            _orderService = orderService;
            _config = config;
        }

        [Authorize(Policy = "UserPolicy")]
        [HttpPost]
        public async Task<IActionResult> CreateOrder(OrderApiRequestModel orderRequestModel)
        {
            try
            {
                var result = await _orderService.CreateOrder(orderRequestModel);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Policy = "UserPolicy")]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetOrdersByUser(Guid userId)
        {
            var result = await _orderService.GetOrdersByUserId(userId);
            return Ok(result);
        }

        [HttpGet("orderNumber/{orderNumber}")]
        public async Task<IActionResult> GetOrderByOrderNo(string orderNumber)
        {
            var result = await _orderService.GetOrderByOrderNumber(orderNumber);
            return Ok(result);
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var result = await _orderService.GetAllOrdersAsync();
            return Ok(result);
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(Order order)
        {
            try
            {
                var result = await _orderService.UpdateOrderById(order.OrderId, order);
                
                return Ok(result);
            } 
            catch (Exception ex)
            {
                var response = new ResponseModel<string>
                {
                    StatusCode = 500,
                    Status = "Error",
                    Message = ex.Message,
                };

                return StatusCode(500, new { status = response.Status, message = response.Message });
            }
        }

        [HttpGet("{orderId}/shipping-info")]
        public async Task<IActionResult> GetShippingInfo(Guid orderId)
        {
            var result = await _orderService.GetShippingInfo(orderId);
            return Ok(result);
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpPut("{orderId}/shipping-info")]
        public async Task<IActionResult> UpdateShippingInfo(Guid orderId, ShippingInfoModel shippingInfoModel)
        {
            var result = await _orderService.UpdateShippingInfo(orderId, shippingInfoModel);
            return Ok(result);
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpPut("{orderId}/status-and-shipping-info")]
        public async Task<IActionResult> UpdateOrderStatusAndShippingInfo(Guid orderId, [FromBody] UpdateOrderStatusAndShippingInfoRequest payload)
        {
            var result = await _orderService.UpdateOrderStatusAndShippingInfo(orderId, payload.OrderStatus, payload.ShippingInfo);
            return Ok(result);
        }
    }
}

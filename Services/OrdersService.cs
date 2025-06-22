using ApplicationToSellThings.APIs.Areas.Identity.Data;
using ApplicationToSellThings.APIs.Data;
using ApplicationToSellThings.APIs.Models;
using ApplicationToSellThings.APIs.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace ApplicationToSellThings.APIs.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly ApplicationToSellThingsAPIsContext _dbContext;
        private readonly IAddressService _addressService;
        private readonly IStatusService _statusService;
        private readonly EmailService _emailService;
        private readonly UserManager<ApplicationToSellThingsAPIsUser> userManager;
        public OrdersService(ApplicationToSellThingsAPIsContext dbContext, IAddressService addressService, IStatusService statusService, 
            EmailService emailService, UserManager<ApplicationToSellThingsAPIsUser> userManager)
        {
            _dbContext = dbContext;
            _addressService = addressService;
            _statusService = statusService;
            _emailService = emailService;
            this.userManager = userManager;
        }

        public async Task<ResponseModel<OrderApiResponseModel>> CreateOrder(OrderApiRequestModel orderRequestModel)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Generate a unique order number
                string orderNumber = GenerateOrderNumber();
                int orderStatusId = await GetDeliveryStatusIdByAlias("S_PENDING");
                int deliveryStatusId = await GetDeliveryStatusIdByAlias("S_NOTSHIPPED");
                string userId = orderRequestModel.UserId.ToString();
                var user = await userManager.FindByIdAsync(userId);
                var shippingInfo = new ShippingInfoModel
                {
                    ShippingInfoId = Guid.NewGuid(),
                    StatusId = deliveryStatusId
                };
                _dbContext.ShippingInfos.Add(shippingInfo);
                await _dbContext.SaveChangesAsync();

                // Retrieve the shipping address
                var shippingAddress = await _addressService.GetAddressById(orderRequestModel.ShippingAddressId);
                var orderStatusInfo = await _statusService.GetStatusById(orderStatusId);
                
                if (shippingAddress == null)
                {
                    return new ResponseModel<OrderApiResponseModel>
                    {
                        StatusCode = 404,
                        Status = "Error",
                        Message = "Shipping address not found",
                    };
                }

                var orderData = new Order
                    {
                        OrderId = Guid.NewGuid(),
                        UserId = orderRequestModel.UserId,
                        OrderNumber = orderNumber,
                        PaymentMethod = orderRequestModel.PaymentMethod,
                        CardId = orderRequestModel.CardId,
                        TotalAmount = 0, // This will be calculated later
                        Tax = 0,
                        OrderStatus = orderStatusInfo.Data.Name,
                        OrderCreatedAt = DateTime.UtcNow,
                        ShippingInfoId = shippingInfo.ShippingInfoId,
                        ShippingAddressId = shippingAddress.Data.Id,
                        ShippingMethod = orderRequestModel.ShippingMethod,
                        OrderDetails = new List<OrderDetail>()
                    };

                    decimal totalAmount = 0;

                    foreach (var productModel in orderRequestModel.Products)
                    {
                        var product = await _dbContext.Products.FindAsync(productModel.ProductId);
                        if (product == null)
                        {
                            return new ResponseModel<OrderApiResponseModel>
                            {
                                StatusCode = 404,
                                Status = "Error",
                                Message = $"Product with ID {productModel.ProductId} not found",
                            };
                        }

                        var orderDetail = new OrderDetail
                        {
                            OrderDetailId = Guid.NewGuid(),
                            OrderId = orderData.OrderId,
                            ProductId = product.ProductId,
                            AddressId = orderRequestModel.ShippingAddressId,
                            Quantity = productModel.Quantity,
                            Total = (decimal)product.Price * productModel.Quantity,
                            Product = product
                        };

                        totalAmount += orderDetail.Total;

                        orderData.OrderDetails.Add(orderDetail);
                    }

                    orderData.TotalAmount = totalAmount;

                    _dbContext.Orders.Add(orderData);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                var response = new ResponseModel<OrderApiResponseModel>()
                {
                    StatusCode = 200,
                    Status = "Success",
                    Message = "Order Created Successfully",
                    Data = new OrderApiResponseModel
                    {
                        OrderNumber = orderData.OrderNumber,
                        PaymentMethod = orderData.PaymentMethod,
                        TotalAmount = orderData.TotalAmount,
                        Tax = orderData.Tax,
                        OrderStatus = orderData.OrderStatus,
                        OrderCreatedAt = orderData.OrderCreatedAt,
                        ShippingMethod = orderData.ShippingMethod,
                        OrderDetails = orderData.OrderDetails.Select(od => new OrderDetailApiResponseModel
                        {
                            ProductId = od.ProductId,
                            Quantity = od.Quantity,
                            Total = od.Total,
                            ProductName = od.Product.ProductName
                        }).ToList()
                    },
                };

                // Send confirmation email
                var emailBody = $@"
                        <h3>Order Confirmation</h3>
                        <p>Thank you for your order !</p>
                        <p>Order Number: {response.Data.OrderNumber}</p>
                        <p>Total Amount: ${response.Data.TotalAmount}</p>
                    ";
                await _emailService.SendEmailAsync(user.Email, "Order Confirmation", emailBody);
                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ResponseModel<OrderApiResponseModel>
                {
                    StatusCode = 500,
                    Status = "Error",
                    Message = ex.Message.ToString(),
                };
            }
        }

        public async Task<ResponseModel<Order>> GetOrdersByUserId(Guid userId)
        {
            try
            {
                string id = userId.ToString();
                var orders = await _dbContext.Orders
                    .Include(o => o.OrderDetails)                                    
                    .ThenInclude(od => od.Product)
                    .Include(os => os.ShippingInfo)
                    .ThenInclude(st => st.DeliveryStatus)
                    .Where(o => o.UserId == userId)
                    .ToListAsync();

                if (orders != null)
                {
                    var response = new ResponseModel<Order>()
                    {
                        StatusCode = 200,
                        Status = "Success",
                        Message = "List of orders found",
                        Items = orders,
                    };

                    return response;
                }
                else
                {
                    return new ResponseModel<Order>
                    {
                        StatusCode = 404,
                        Status = "Not Found",
                        Message = "Orders Not Found!",
                        Items = null,
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<Order>
                {
                    StatusCode = 500,
                    Status = "Error",
                    Message = ex.Message.ToString(),
                };
            }
        }

        public async Task<ResponseModel<Order>> GetOrderByOrderNumber(string orderNumber)
        {
            try
            {
                var order = await _dbContext.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .Include(os => os.ShippingInfo)
                    .ThenInclude(st => st.DeliveryStatus)
                    .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);


                if (order != null)
                {
                    var response = new ResponseModel<Order>()
                    {
                        StatusCode = 200,
                        Status = "Success",
                        Message = "Order found!",

                        Data = order,
                    };

                    return response;
                }
                else
                {
                    return new ResponseModel<Order>
                    {
                        StatusCode = 404,
                        Status = "Not Found",
                        Message = "Orders Not Found!",
                        Items = null,
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<Order>
                {
                    StatusCode = 500,
                    Status = "Error",
                    Message = ex.Message.ToString(),
                };
            }
        }


        public async Task<ResponseModel<Order>> GetAllOrdersAsync()
        {
            try
            {
                var orders = await _dbContext.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .Include(o => o.ShippingInfo)
                        .ThenInclude(si => si.DeliveryStatus) // Include DeliveryStatus
                    .ToListAsync();


                if (orders != null)
                {
                    foreach (var order in orders)
                    {
                        foreach (var orderDetail in order.OrderDetails)
                        {
                            var addresses = await _addressService.GetAddressByUser(order.UserId);
                            if (addresses.StatusCode == 200 && addresses.Items != null)
                            {
                                foreach (var address in addresses.Items)
                                {
                                    if (orderDetail.AddressId == address.Id)
                                    {
                                        orderDetail.Address = address;
                                    }
                                }
                            }
                        }
                    }
                    var response = new ResponseModel<Order>()
                    {
                        StatusCode = 200,
                        Status = "Success",
                        Message = "List of orders found",
                        Items = orders,
                    };

                    return response;
                }
                else
                {
                    return new ResponseModel<Order>
                    {
                        StatusCode = 404,
                        Status = "Not Found",
                        Message = "Orders Not Found!",
                        Items = null,
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<Order>
                {
                    StatusCode = 500,
                    Status = "Error",
                    Message = ex.Message.ToString(),
                };
            }
        }

        public async Task<ResponseModel<Order>> UpdateOrderById(Guid orderId, Order orderModel)
        {
            try
            {
                var order = await _dbContext.Orders
                    .Include(o => o.OrderDetails)
                    .Include(o => o.ShippingInfo)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);
                
                if (order == null)
                {
                    return new ResponseModel<Order>
                    {
                        StatusCode = 404,
                        Status = "Not Found",
                        Message = "Order not found"
                    };
                }

                order.UserId = orderModel.UserId;
                order.PaymentMethod = orderModel.PaymentMethod;
                order.CardId = orderModel.CardId;
                order.OrderStatus = orderModel.OrderStatus;
                order.Tax = orderModel.Tax;
                order.OrderCreatedAt = orderModel.OrderCreatedAt;
                order.OrderUpdatedAt = DateTime.Now;

                if (orderModel.ShippingInfo != null)
                {
                    order.ShippingInfo.ShippingDate = orderModel.ShippingInfo.ShippingDate;
                    order.ShippingInfo.EstimatedDeliveryDate = orderModel.ShippingInfo.EstimatedDeliveryDate;
                    order.ShippingInfo.ActualDeliveryDate = orderModel.ShippingInfo.ActualDeliveryDate;
                    order.ShippingInfo.StatusId = orderModel.ShippingInfo.StatusId;
                }

                // Update ShippingAddress
                if (orderModel.ShippingAddressId != Guid.Empty)
                {
                    var shippingAddress = await _addressService.GetAddressById(orderModel.ShippingAddressId);
                    if (shippingAddress == null)
                    {
                        return new ResponseModel<Order>
                        {
                            StatusCode = 404,
                            Status = "Not Found",
                            Message = "Shipping address not found"
                        };
                    }
                    order.ShippingAddressId = shippingAddress.Data.Id;
                }

                var existingOrderDetails = order.OrderDetails.ToList();

                foreach (var orderDetail in orderModel.OrderDetails)
                {
                    var existingOrderDetail = existingOrderDetails.FirstOrDefault(od => od.OrderDetailId == orderDetail.OrderDetailId);
                    if (existingOrderDetail != null)
                    {
                        // Update existing order detail
                        existingOrderDetail.ProductId = orderDetail.ProductId;
                        existingOrderDetail.AddressId = orderDetail.AddressId;
                        existingOrderDetail.Quantity = orderDetail.Quantity;
                        existingOrderDetail.Total = orderDetail.Quantity * orderDetail.Product.Price;
                    }
                    else
                    {
                        // Add new order detail
                        var newOrderDetail = new OrderDetail
                        {
                            OrderDetailId = orderDetail.OrderDetailId != Guid.Empty ? orderDetail.OrderDetailId : Guid.NewGuid(),
                            OrderId = order.OrderId,
                            ProductId = orderDetail.ProductId,
                            AddressId = orderDetail.AddressId,
                            Quantity = orderDetail.Quantity,
                            Total = orderDetail.Quantity * orderDetail.Product.Price,
                        };
                        order.OrderDetails.Add(newOrderDetail);
                    }
                }
                
                // Remove order details that are not in the updated list
                foreach (var existingOrderDetail in existingOrderDetails)
                {
                    if (!orderModel.OrderDetails.Any(od => od.OrderDetailId == existingOrderDetail.OrderDetailId))
                    {
                        _dbContext.OrderDetails.Remove(existingOrderDetail);
                    }
                }

                // Calculate total amount
                order.TotalAmount = order.OrderDetails.Sum(od => od.Total);

                _dbContext.Orders.Update(order);
                await _dbContext.SaveChangesAsync();

                return new ResponseModel<Order>
                {
                    StatusCode = 200,
                    Status = "Success",
                    Data = order
                };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return new ResponseModel<Order>
                {
                    StatusCode = 409,
                    Status = "Concurrency Error",
                    Message = "The record you attempted to edit was modified by another user after you got the original value. The edit operation was canceled."
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<Order>
                {
                    StatusCode = 500,
                    Status = "Error",
                    Message = ex.Message
                };
            }
        }


        public async Task<ResponseModel<ShippingInfoModel>> GetShippingInfo(Guid orderId)
        {
            try
            {
                var order = await _dbContext.Orders
                    .Include(o => o.ShippingInfo)
                    .ThenInclude(st => st.DeliveryStatus)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null || order.ShippingInfo == null)
                {
                    return new ResponseModel<ShippingInfoModel>
                    {
                        StatusCode = 404,
                        Status = "Not Found",
                        Message = "Order or ShippingInfo not found"
                    };
                }

                return new ResponseModel<ShippingInfoModel>
                {
                    StatusCode = 200,
                    Status = "Success",
                    Message = "ShippingInfo retrieved successfully",
                    Data = order.ShippingInfo
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<ShippingInfoModel>
                {
                    StatusCode = 500,
                    Status = "Error",
                    Message = ex.Message
                };
            }
        }


        public async Task<ResponseModel<ShippingInfoModel>> UpdateShippingInfo(Guid orderId, ShippingInfoModel shippingInfoModel)
        {
            try
            {
                var order = await _dbContext.Orders
                    .Include(o => o.ShippingInfo)
                    .ThenInclude(st => st.DeliveryStatus)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null || order.ShippingInfo == null)
                {
                    return new ResponseModel<ShippingInfoModel>
                    {
                        StatusCode = 404,
                        Status = "Not Found",
                        Message = "Order or ShippingInfo not found"
                    };
                }

                // Update ShippingInfo properties
                order.ShippingInfo.ShippingDate = shippingInfoModel.ShippingDate;
                order.ShippingInfo.EstimatedDeliveryDate = shippingInfoModel.EstimatedDeliveryDate;
                order.ShippingInfo.ActualDeliveryDate = shippingInfoModel.ActualDeliveryDate;
                order.ShippingInfo.DeliveryStatus = shippingInfoModel.DeliveryStatus;

                // Save changes to the database
                _dbContext.ShippingInfos.Update(order.ShippingInfo);
                await _dbContext.SaveChangesAsync();

                return new ResponseModel<ShippingInfoModel>
                {
                    StatusCode = 200,
                    Status = "Success",
                    Message = "ShippingInfo updated successfully",
                    Data = order.ShippingInfo
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<ShippingInfoModel>
                {
                    StatusCode = 500,
                    Status = "Error",
                    Message = ex.Message
                };
            }
        }


        public async Task<ResponseModel<Order>> UpdateOrderStatusAndShippingInfo(Guid orderId, string newStatus, ShippingInfoModel? shippingInfoUpdates = null)
        {
            try
            {
                var order = await _dbContext.Orders
                    .Include(o => o.ShippingInfo)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    return new ResponseModel<Order>
                    {
                        StatusCode = 404,
                        Status = "Not Found",
                        Message = "Order not found"
                    };
                }

                // Update Order Status if provided
                if (!string.IsNullOrEmpty(newStatus))
                {
                    order.OrderStatus = newStatus;
                    order.OrderUpdatedAt = DateTime.UtcNow;
                }

                // Update ShippingInfo if provided
                if (shippingInfoUpdates != null && order.ShippingInfo != null)
                {
                    order.ShippingInfo.ShippingDate = shippingInfoUpdates.ShippingDate ?? order.ShippingInfo.ShippingDate;
                    order.ShippingInfo.EstimatedDeliveryDate = shippingInfoUpdates.EstimatedDeliveryDate ?? order.ShippingInfo.EstimatedDeliveryDate;
                    order.ShippingInfo.ActualDeliveryDate = shippingInfoUpdates.ActualDeliveryDate ?? order.ShippingInfo.ActualDeliveryDate;

                    // Update DeliveryStatus if needed
                    if (shippingInfoUpdates.StatusId > 0 && shippingInfoUpdates.StatusId != order.ShippingInfo.StatusId)
                    {
                        order.ShippingInfo.StatusId = shippingInfoUpdates.StatusId;
                    }
                }

                // Apply conditional updates based on Order Status
                if (order.ShippingInfo != null)
                {
                    switch (newStatus)
                    {
                        case "Dispatched":
                            order.ShippingInfo.ShippingDate = DateTime.UtcNow;
                            order.ShippingInfo.StatusId = await GetDeliveryStatusIdByAlias("S_INTRANSIT");
                            break;

                        case "Delivered":
                            order.ShippingInfo.ActualDeliveryDate = DateTime.UtcNow;
                            order.ShippingInfo.StatusId = await GetDeliveryStatusIdByAlias("S_DELIVERED");
                            break;

                        case "Cancelled":
                            order.ShippingInfo.StatusId = await GetDeliveryStatusIdByAlias("S_DELIVERYFAILED");
                            break;

                        default:
                            // Optional: Add logging or validation for unsupported statuses
                            break;
                    }
                }

                _dbContext.Orders.Update(order);
                await _dbContext.SaveChangesAsync();

                return new ResponseModel<Order>
                {
                    StatusCode = 200,
                    Status = "Success",
                    Message = "Order and ShippingInfo updated successfully",
                    Data = order
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<Order>
                {
                    StatusCode = 500,
                    Status = "Error",
                    Message = ex.Message
                };
            }
        }



        /* Private Methods */

        private string GenerateOrderNumber()
        {
            // Get the current date in YYYYMMDD format
            string datePart = DateTime.UtcNow.ToString("yyyyMMdd");

            // Generate a random alphanumeric string (e.g., 6 characters long)
            string randomPart = GenerateRandomAlphanumericString(6);

            // Combine parts into a final order number
            return $"{datePart}-{randomPart}";
        }

        private string GenerateRandomAlphanumericString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task<int> GetDeliveryStatusIdByAlias(string alias)
        {
            var deliveryStatus = await _dbContext.Status
                .FirstOrDefaultAsync(ds => ds.Alias == alias);

            if (deliveryStatus == null)
            {
                throw new Exception($"Delivery status with alias '{alias}' not found.");
            }

            return deliveryStatus.StatusId;
        }
    }
}

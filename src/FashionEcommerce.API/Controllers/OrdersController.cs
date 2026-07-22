using FashionEcommerce.Core.Entities;
using FashionEcommerce.Services.Interfaces;
using FashionEcommerce.Services.Models.Orders;
using FashionEcommerce.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FashionEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Place a new order from the current user's cart.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderDto>> PlaceOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Order request is required.");
                }

                var userId = GetCurrentUserId();
                var order = new Order
                {
                    UserId = userId,
                    ShippingAddress = request.ShippingAddress,
                    City = request.City,
                    State = request.State,
                    PostalCode = request.PostalCode,
                    Country = request.Country,
                    PhoneNumber = request.PhoneNumber,
                    PaymentMethod = request.PaymentMethod,
                    Notes = request.Notes
                };

                var createdOrder = await _orderService.CreateOrderAsync(order);
                var orderWithDetails = await _orderService.GetOrderByIdAsync(createdOrder.Id);
                return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.Id }, MapOrder(orderWithDetails ?? createdOrder));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Order validation failed");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating order");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get order details by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound("Order not found.");
                }

                var userId = GetCurrentUserId();
                if (order.UserId != userId && !IsAdminManagerOrStaff())
                {
                    return Forbid();
                }

                return Ok(MapOrder(order));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get all orders with pagination, filters, and sorting. Admin/Manager/Staff only.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<OrderDto>>> GetAllOrders(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] OrderStatus? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = null)
        {
            try
            {
                if (!IsAdminManagerOrStaff())
                {
                    return Forbid();
                }

                var orders = await _orderService.GetAllOrdersAsync(new OrderQueryParameters
                {
                    Page = page,
                    PageSize = pageSize,
                    Status = status,
                    FromDate = fromDate,
                    ToDate = toDate,
                    Search = search,
                    SortBy = sortBy,
                    SortDirection = sortDirection
                });

                return Ok(MapPagedOrders(orders));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get current user's orders with pagination, filters, and sorting.
        /// </summary>
        [HttpGet("my")]
        public async Task<ActionResult<PagedResult<OrderDto>>> GetMyOrders(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] OrderStatus? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = null)
        {
            try
            {
                var orders = await _orderService.GetUserOrdersAsync(GetCurrentUserId(), new OrderQueryParameters
                {
                    Page = page,
                    PageSize = pageSize,
                    Status = status,
                    FromDate = fromDate,
                    ToDate = toDate,
                    Search = search,
                    SortBy = sortBy,
                    SortDirection = sortDirection
                });

                return Ok(MapPagedOrders(orders));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user's orders");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Cancel a pending or processing order.
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound("Order not found.");
                }

                var userId = GetCurrentUserId();
                if (order.UserId != userId && !IsAdminManagerOrStaff())
                {
                    return Forbid();
                }

                var result = await _orderService.CancelOrderAsync(id);
                if (!result)
                {
                    return BadRequest("Cancel order failed.");
                }

                return Ok(new { message = "Order cancelled successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update order status. Admin/Manager/Staff only.
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<OrderDto>> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required.");
                }

                var updatedOrder = await _orderService.UpdateOrderStatusAsync(id, request.Status);
                var orderWithDetails = await _orderService.GetOrderByIdAsync(updatedOrder.Id);
                return Ok(MapOrder(orderWithDetails ?? updatedOrder));
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Order not found.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("Cannot identify current user.");
            }

            return userId;
        }

        private bool IsAdminManagerOrStaff()
        {
            return User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Staff");
        }

        private static PagedResult<OrderDto> MapPagedOrders(PagedResult<Order> orders)
        {
            return new PagedResult<OrderDto>
            {
                Page = orders.Page,
                PageSize = orders.PageSize,
                TotalItems = orders.TotalItems,
                Items = orders.Items.Select(MapOrder).ToList()
            };
        }

        private static OrderDto MapOrder(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                CustomerName = order.User == null ? null : $"{order.User.FirstName} {order.User.LastName}".Trim(),
                CustomerEmail = order.User?.Email,
                OrderNumber = order.OrderNumber,
                Status = order.Status,
                SubTotal = order.SubTotal,
                ShippingCost = order.ShippingCost,
                TaxAmount = order.TaxAmount,
                DiscountAmount = order.DiscountAmount,
                TotalPrice = order.TotalPrice,
                PaymentMethod = order.PaymentMethod,
                ShippingAddress = order.ShippingAddress,
                City = order.City,
                State = order.State,
                PostalCode = order.PostalCode,
                Country = order.Country,
                PhoneNumber = order.PhoneNumber,
                Notes = order.Notes,
                ShippedDate = order.ShippedDate,
                DeliveredDate = order.DeliveredDate,
                TrackingNumber = order.TrackingNumber,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order.Items.Select(item => new OrderItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name,
                    SKU = item.Product?.SKU,
                    ImageUrl = item.Product?.ImageUrl,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    Size = item.Size,
                    Color = item.Color
                }).ToList(),
                Shipment = order.Shipment == null ? null : new OrderShipmentDto
                {
                    Id = order.Shipment.Id,
                    CarrierName = order.Shipment.CarrierName,
                    TrackingNumber = order.Shipment.TrackingNumber,
                    Status = order.Shipment.Status,
                    ShippingFee = order.Shipment.ShippingFee,
                    EstimatedDeliveryDate = order.Shipment.EstimatedDeliveryDate,
                    ShippedAt = order.Shipment.ShippedAt,
                    DeliveredAt = order.Shipment.DeliveredAt
                }
            };
        }
    }

    public class CreateOrderRequest
    {
        [Required]
        [StringLength(500)]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateOrderStatusRequest
    {
        [Required]
        public OrderStatus Status { get; set; }
    }
}

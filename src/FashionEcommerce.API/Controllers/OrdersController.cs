using FashionEcommerce.Core.Entities;
using FashionEcommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

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

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("Không xác định được ID người dùng.");
            }
            return userId;
        }

        /// <summary>
        /// Place a new order (Checkout)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Order>> PlaceOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Yêu cầu đặt hàng không hợp lệ.");
                }

                int userId = GetCurrentUserId();

                var order = new Order
                {
                    UserId = userId,
                    ShippingAddress = request.ShippingAddress,
                    City = request.City,
                    State = request.State,
                    PostalCode = request.PostalCode,
                    Country = request.Country,
                    PhoneNumber = request.PhoneNumber,
                    Notes = request.Notes
                };

                var createdOrder = await _orderService.CreateOrderAsync(order);
                return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.Id }, createdOrder);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Lỗi kiểm tra tồn kho hoặc giỏ hàng trống");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong quá trình đặt hàng");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get order details by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound("Không tìm thấy đơn hàng.");
                }

                int userId = GetCurrentUserId();
                bool isAdminOrManagerOrStaff = IsAdminManagerOrStaff();

                if (order.UserId != userId && !isAdminOrManagerOrStaff)
                {
                    return Forbid("Bạn không có quyền xem đơn hàng này.");
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin đơn hàng {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get all orders (Admin/Manager/Staff only)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
        {
            try
            {
                if (!IsAdminManagerOrStaff())
                {
                    return Forbid("Bạn không có quyền truy cập danh sách toàn bộ đơn hàng.");
                }

                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách đơn hàng");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get order history for the current user
        /// </summary>
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<Order>>> GetMyOrders()
        {
            try
            {
                int userId = GetCurrentUserId();
                var orders = await _orderService.GetUserOrdersAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách đơn hàng của tôi");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Cancel a pending order
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound("Không tìm thấy đơn hàng.");
                }

                int userId = GetCurrentUserId();
                bool isAdminOrManagerOrStaff = IsAdminManagerOrStaff();

                if (order.UserId != userId && !isAdminOrManagerOrStaff)
                {
                    return Forbid("Bạn không có quyền hủy đơn hàng này.");
                }

                bool result = await _orderService.CancelOrderAsync(id);
                if (!result)
                {
                    return BadRequest("Hủy đơn hàng thất bại.");
                }

                return Ok(new { message = "Hủy đơn hàng thành công." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Lỗi hủy đơn hàng do trạng thái không hợp lệ");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hủy đơn hàng {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update order status (Admin/Manager/Staff only)
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<Order>> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Yêu cầu cập nhật không hợp lệ.");
                }

                var updatedOrder = await _orderService.UpdateOrderStatusAsync(id, request.Status);
                return Ok(updatedOrder);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái đơn hàng {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        private bool IsAdminManagerOrStaff()
        {
            return User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Staff");
        }
    }

    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc.")]
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

        [Required(ErrorMessage = "Số điện thoại nhận hàng là bắt buộc.")]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateOrderStatusRequest
    {
        [Required]
        public OrderStatus Status { get; set; }
    }
}

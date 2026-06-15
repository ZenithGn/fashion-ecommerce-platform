using FashionEcommerce.Core.Entities;
using FashionEcommerce.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FashionEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        // GET api/cart/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<Cart?>> GetCartByUserId(int userId)
        {
            try
            {
                var cart = await _cartService.GetCartByUserIdAsync(userId);
                if (cart == null) return NotFound();
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST api/cart/user/{userId}/items
        [HttpPost("user/{userId}/items")]
        public async Task<ActionResult<Cart>> AddToCart(int userId, [FromBody] AddToCartRequest request)
        {
            try
            {
                var cart = await _cartService.AddToCartAsync(userId, request.ProductId, request.Quantity);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product {ProductId} to cart for user {UserId}", request?.ProductId, userId);
                return BadRequest(ex.Message);
            }
        }

        // PUT api/cart/{cartId}/items/{productId}
        [HttpPut("{cartId}/items/{productId}")]
        public async Task<ActionResult<Cart>> UpdateCartItem(int cartId, int productId, [FromBody] UpdateCartItemRequest request)
        {
            try
            {
                var cart = await _cartService.UpdateCartItemAsync(cartId, productId, request.Quantity);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item {ProductId} in cart {CartId}", productId, cartId);
                return BadRequest(ex.Message);
            }
        }

        // DELETE api/cart/{cartId}/items/{productId}
        [HttpDelete("{cartId}/items/{productId}")]
        public async Task<ActionResult<Cart>> RemoveFromCart(int cartId, int productId)
        {
            try
            {
                var cart = await _cartService.RemoveFromCartAsync(cartId, productId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing product {ProductId} from cart {CartId}", productId, cartId);
                return BadRequest(ex.Message);
            }
        }

        // DELETE api/cart/{cartId}
        [HttpDelete("{cartId}")]
        public async Task<ActionResult> ClearCart(int cartId)
        {
            try
            {
                var ok = await _cartService.ClearCartAsync(cartId);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart {CartId}", cartId);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET api/cart/{cartId}/total
        [HttpGet("{cartId}/total")]
        public async Task<ActionResult<object>> GetCartTotal(int cartId)
        {
            try
            {
                var total = await _cartService.CalculateCartTotalAsync(cartId);
                return Ok(new { cartId = cartId, total = total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total for cart {CartId}", cartId);
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartItemRequest
    {
        public int Quantity { get; set; }
    }
}

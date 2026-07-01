using FashionEcommerce.Core.Entities;
using FashionEcommerce.Services.Products;
using FashionEcommerce.Services.Categories;

namespace FashionEcommerce.Services.Interfaces
{
    /// <summary>
    /// Interface for User service
    /// </summary>
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> GetUsersAsync(string? search, int? roleId, bool? isActive);
        Task<User> CreateUserAsync(User user);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int userId);
        Task<User?> LockUserAsync(int userId);
        Task<User?> UnlockUserAsync(int userId);
        Task<User?> UpdateUserRoleAsync(int userId, int roleId);
    }

    /// <summary>
    /// Interface for Product service
    /// </summary>
    public interface IProductService
    {
        Task<PagedResult<ProductListDto>> GetProductsAsync(ProductQueryParameters parameters);
        Task<ProductDetailDto?> GetProductByIdAsync(int productId);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<ProductServiceResult<PagedResult<ProductSearchDto>>> SearchProductsAsync(SearchProductQueryParameters parameters);
        Task<ProductServiceResult<ProductDetailDto>> CreateProductAsync(CreateProductDto dto);
        Task<ProductServiceResult<Product>> UpdateProductAsync(int id, Product product);
        Task<bool> DeleteProductAsync(int productId);
    }

    /// <summary>
    /// Interface for Category service
    /// </summary>
    public interface ICategoryService
    {
        Task<Category?> GetCategoryByIdAsync(int categoryId);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentCategoryId);
        Task<CategoryServiceResult<Category>> CreateCategoryAsync(CreateCategoryDto dto);
        Task<CategoryServiceResult<Category>> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
        Task<bool> DeleteCategoryAsync(int categoryId);
    }

    /// <summary>
    /// Interface for Inventory service
    /// </summary>
    public interface IInventoryService
    {
        Task<Inventory?> GetInventoryByIdAsync(int inventoryId);
        Task<Inventory?> GetInventoryByProductIdAsync(int productId);
        Task<int> GetAvailableQuantityAsync(int productId);
        Task<bool> CheckAvailabilityAsync(int productId, int quantity);
        Task<Inventory> UpdateInventoryAsync(int productId, int quantity);
        Task<IEnumerable<Inventory>> GetAllInventoriesAsync();
        Task<Inventory> CreateOrUpdateInventoryAsync(Inventory inventory);
        Task<bool> DeleteInventoryAsync(int inventoryId);
        Task<bool> ReserveInventoryAsync(int productId, int quantity);
        Task<bool> ReleaseReservationAsync(int productId, int quantity);
    }

    /// <summary>
    /// Interface for Cart service
    /// </summary>
    public interface ICartService
    {
        Task<Cart?> GetCartByUserIdAsync(int userId);
        Task<Cart> AddToCartAsync(int userId, int productId, int quantity);
        Task<Cart> UpdateCartItemAsync(int cartId, int productId, int quantity);
        Task<Cart> RemoveFromCartAsync(int cartId, int productId);
        Task<bool> ClearCartAsync(int cartId);
        Task<decimal> CalculateCartTotalAsync(int cartId);
    }

    /// <summary>
    /// Interface for Order service
    /// </summary>
    public interface IOrderService
    {
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<Order?> GetOrderByNumberAsync(string orderNumber);
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order> CreateOrderAsync(Order order);
        Task<Order> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<bool> CancelOrderAsync(int orderId);
    }

    /// <summary>
    /// Interface for Shipment service
    /// </summary>
    public interface IShipmentService
    {
        Task<IEnumerable<Shipment>> GetShipmentsAsync(ShipmentStatus? status, string? carrier, DateTime? from, DateTime? to);
        Task<Shipment?> GetShipmentByIdAsync(int id);
        Task<Shipment?> GetShipmentByOrderIdAsync(int orderId);
        Task<Shipment> CreateShipmentAsync(int orderId, string carrierName, string? trackingNumber, decimal shippingFee, DateTime? estimatedDeliveryDate, string? notes);
        Task<Shipment> UpdateShipmentAsync(int id, string carrierName, string? trackingNumber, decimal shippingFee, DateTime? estimatedDeliveryDate, string? notes);
        Task<Shipment> UpdateShipmentStatusAsync(int id, ShipmentStatus status, string? location, string? note, DateTime? occurredAt);
        Task<Shipment> AddShipmentEventAsync(int shipmentId, ShipmentStatus status, string? location, string? note, DateTime? occurredAt);
        Task<IEnumerable<ShipmentEvent>> GetShipmentEventsAsync(int shipmentId);
    }
}

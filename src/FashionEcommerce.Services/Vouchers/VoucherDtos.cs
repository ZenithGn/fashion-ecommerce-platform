using FashionEcommerce.Core.Entities;

namespace FashionEcommerce.Services.Vouchers
{
    public class VoucherDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public VoucherDiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public decimal? MaximumDiscountAmount { get; set; }
        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateVoucherDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public VoucherDiscountType DiscountType { get; set; } = VoucherDiscountType.FixedAmount;
        public decimal DiscountValue { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public decimal? MaximumDiscountAmount { get; set; }
        public int? UsageLimit { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateVoucherDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public VoucherDiscountType? DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public decimal? MaximumDiscountAmount { get; set; }
        public int? UsageLimit { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ValidateVoucherRequest
    {
        public string Code { get; set; } = string.Empty;
        public decimal OrderAmount { get; set; }
    }

    public class VoucherValidationResultDto
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public int? VoucherId { get; set; }
        public string? Code { get; set; }
        public decimal OrderAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
    }

    public class VoucherServiceResult<T>
    {
        public bool Succeeded { get; set; }
        public string? ErrorMessage { get; set; }
        public VoucherServiceError Error { get; set; } = VoucherServiceError.None;
        public T? Data { get; set; }

        public static VoucherServiceResult<T> Success(T data) => new() { Succeeded = true, Data = data };

        public static VoucherServiceResult<T> Failure(VoucherServiceError error, string message) =>
            new() { Succeeded = false, Error = error, ErrorMessage = message };
    }

    public enum VoucherServiceError
    {
        None,
        Validation,
        NotFound,
        Conflict
    }
}

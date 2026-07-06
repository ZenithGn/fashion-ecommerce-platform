using FashionEcommerce.Core.Entities;
using FashionEcommerce.Data;
using FashionEcommerce.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FashionEcommerce.Services.Vouchers
{
    public class VoucherService : IVoucherService
    {
        private readonly FashionEcommerceDbContext _context;

        public VoucherService(FashionEcommerceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VoucherDto>> GetAllVouchersAsync(bool includeInactive = true)
        {
            var query = _context.Vouchers
                .AsNoTracking()
                .Where(v => !v.IsDeleted);

            if (!includeInactive)
                query = query.Where(v => v.IsActive);

            return await query
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => ToDto(v))
                .ToListAsync();
        }

        public async Task<VoucherDto?> GetVoucherByIdAsync(int id)
        {
            return await _context.Vouchers
                .AsNoTracking()
                .Where(v => v.Id == id && !v.IsDeleted)
                .Select(v => ToDto(v))
                .FirstOrDefaultAsync();
        }

        public async Task<VoucherDto?> GetVoucherByCodeAsync(string code)
        {
            var normalizedCode = NormalizeCode(code);
            if (string.IsNullOrWhiteSpace(normalizedCode))
                return null;

            return await _context.Vouchers
                .AsNoTracking()
                .Where(v => v.Code == normalizedCode && !v.IsDeleted)
                .Select(v => ToDto(v))
                .FirstOrDefaultAsync();
        }

        public async Task<VoucherServiceResult<VoucherDto>> CreateVoucherAsync(CreateVoucherDto dto)
        {
            var validationMessage = ValidateCreateDto(dto);
            if (validationMessage != null)
                return VoucherServiceResult<VoucherDto>.Failure(VoucherServiceError.Validation, validationMessage);

            var normalizedCode = NormalizeCode(dto.Code);
            var codeExists = await _context.Vouchers.AnyAsync(v => v.Code == normalizedCode && !v.IsDeleted);
            if (codeExists)
                return VoucherServiceResult<VoucherDto>.Failure(VoucherServiceError.Conflict, "Voucher code already exists.");

            var voucher = new Voucher
            {
                Code = normalizedCode,
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                MinimumOrderAmount = dto.MinimumOrderAmount,
                MaximumDiscountAmount = dto.MaximumDiscountAmount,
                UsageLimit = dto.UsageLimit,
                StartDate = EnsureUtc(dto.StartDate),
                EndDate = dto.EndDate.HasValue ? EnsureUtc(dto.EndDate.Value) : null,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            return VoucherServiceResult<VoucherDto>.Success(ToDto(voucher));
        }

        public async Task<VoucherServiceResult<VoucherDto>> UpdateVoucherAsync(int id, UpdateVoucherDto dto)
        {
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
            if (voucher == null)
                return VoucherServiceResult<VoucherDto>.Failure(VoucherServiceError.NotFound, "Voucher not found.");

            if (!string.IsNullOrWhiteSpace(dto.Code))
            {
                var normalizedCode = NormalizeCode(dto.Code);
                var codeExists = await _context.Vouchers.AnyAsync(v => v.Id != id && v.Code == normalizedCode && !v.IsDeleted);
                if (codeExists)
                    return VoucherServiceResult<VoucherDto>.Failure(VoucherServiceError.Conflict, "Voucher code already exists.");

                voucher.Code = normalizedCode;
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
                voucher.Name = dto.Name.Trim();

            if (dto.Description != null)
                voucher.Description = dto.Description.Trim();

            if (dto.DiscountType.HasValue)
                voucher.DiscountType = dto.DiscountType.Value;

            if (dto.DiscountValue.HasValue)
                voucher.DiscountValue = dto.DiscountValue.Value;

            if (dto.MinimumOrderAmount.HasValue)
                voucher.MinimumOrderAmount = dto.MinimumOrderAmount.Value;

            if (dto.MaximumDiscountAmount.HasValue)
                voucher.MaximumDiscountAmount = dto.MaximumDiscountAmount.Value;

            if (dto.UsageLimit.HasValue)
                voucher.UsageLimit = dto.UsageLimit.Value;

            if (dto.StartDate.HasValue)
                voucher.StartDate = EnsureUtc(dto.StartDate.Value);

            if (dto.EndDate.HasValue)
                voucher.EndDate = EnsureUtc(dto.EndDate.Value);

            if (dto.IsActive.HasValue)
                voucher.IsActive = dto.IsActive.Value;

            var validationMessage = ValidateVoucher(voucher);
            if (validationMessage != null)
                return VoucherServiceResult<VoucherDto>.Failure(VoucherServiceError.Validation, validationMessage);

            voucher.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return VoucherServiceResult<VoucherDto>.Success(ToDto(voucher));
        }

        public async Task<bool> DeleteVoucherAsync(int id)
        {
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
            if (voucher == null)
                return false;

            voucher.IsDeleted = true;
            voucher.IsActive = false;
            voucher.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<VoucherValidationResultDto> ValidateVoucherAsync(ValidateVoucherRequest request)
        {
            var normalizedCode = NormalizeCode(request.Code);
            var result = new VoucherValidationResultDto
            {
                Code = normalizedCode,
                OrderAmount = request.OrderAmount,
                FinalAmount = request.OrderAmount
            };

            if (string.IsNullOrWhiteSpace(normalizedCode))
                return Invalid(result, "Voucher code is required.");

            if (request.OrderAmount <= 0)
                return Invalid(result, "Order amount must be greater than 0.");

            var voucher = await _context.Vouchers
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Code == normalizedCode && !v.IsDeleted);

            if (voucher == null)
                return Invalid(result, "Voucher not found.");

            result.VoucherId = voucher.Id;

            var validationMessage = ValidateVoucherForOrder(voucher, request.OrderAmount);
            if (validationMessage != null)
                return Invalid(result, validationMessage);

            var discountAmount = CalculateDiscount(voucher, request.OrderAmount);
            result.IsValid = true;
            result.Message = "Voucher is valid.";
            result.DiscountAmount = discountAmount;
            result.FinalAmount = Math.Max(0, request.OrderAmount - discountAmount);
            return result;
        }

        private static VoucherValidationResultDto Invalid(VoucherValidationResultDto result, string message)
        {
            result.IsValid = false;
            result.Message = message;
            result.DiscountAmount = 0;
            result.FinalAmount = result.OrderAmount;
            return result;
        }

        private static string? ValidateCreateDto(CreateVoucherDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code))
                return "Code is required.";

            if (string.IsNullOrWhiteSpace(dto.Name))
                return "Name is required.";

            var voucher = new Voucher
            {
                Code = NormalizeCode(dto.Code),
                Name = dto.Name.Trim(),
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                MinimumOrderAmount = dto.MinimumOrderAmount,
                MaximumDiscountAmount = dto.MaximumDiscountAmount,
                UsageLimit = dto.UsageLimit,
                StartDate = EnsureUtc(dto.StartDate),
                EndDate = dto.EndDate.HasValue ? EnsureUtc(dto.EndDate.Value) : null,
                IsActive = dto.IsActive
            };

            return ValidateVoucher(voucher);
        }

        private static string? ValidateVoucher(Voucher voucher)
        {
            if (voucher.DiscountValue <= 0)
                return "Discount value must be greater than 0.";

            if (voucher.DiscountType == VoucherDiscountType.Percentage && voucher.DiscountValue > 100)
                return "Percentage discount cannot be greater than 100.";

            if (voucher.MinimumOrderAmount.HasValue && voucher.MinimumOrderAmount.Value < 0)
                return "Minimum order amount cannot be negative.";

            if (voucher.MaximumDiscountAmount.HasValue && voucher.MaximumDiscountAmount.Value <= 0)
                return "Maximum discount amount must be greater than 0.";

            if (voucher.UsageLimit.HasValue && voucher.UsageLimit.Value <= 0)
                return "Usage limit must be greater than 0.";

            if (voucher.EndDate.HasValue && voucher.EndDate.Value <= voucher.StartDate)
                return "End date must be after start date.";

            return null;
        }

        private static string? ValidateVoucherForOrder(Voucher voucher, decimal orderAmount)
        {
            var now = DateTime.UtcNow;

            if (!voucher.IsActive)
                return "Voucher is inactive.";

            if (voucher.StartDate > now)
                return "Voucher is not active yet.";

            if (voucher.EndDate.HasValue && voucher.EndDate.Value < now)
                return "Voucher has expired.";

            if (voucher.UsageLimit.HasValue && voucher.UsedCount >= voucher.UsageLimit.Value)
                return "Voucher usage limit has been reached.";

            if (voucher.MinimumOrderAmount.HasValue && orderAmount < voucher.MinimumOrderAmount.Value)
                return $"Minimum order amount is {voucher.MinimumOrderAmount.Value}.";

            return null;
        }

        private static decimal CalculateDiscount(Voucher voucher, decimal orderAmount)
        {
            var discount = voucher.DiscountType == VoucherDiscountType.Percentage
                ? orderAmount * voucher.DiscountValue / 100
                : voucher.DiscountValue;

            if (voucher.MaximumDiscountAmount.HasValue)
                discount = Math.Min(discount, voucher.MaximumDiscountAmount.Value);

            return Math.Min(discount, orderAmount);
        }

        private static string NormalizeCode(string code)
        {
            return code.Trim().ToUpperInvariant();
        }

        private static DateTime EnsureUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc)
                return value;

            if (value.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(value, DateTimeKind.Utc);

            return value.ToUniversalTime();
        }

        private static VoucherDto ToDto(Voucher voucher)
        {
            return new VoucherDto
            {
                Id = voucher.Id,
                Code = voucher.Code,
                Name = voucher.Name,
                Description = voucher.Description,
                DiscountType = voucher.DiscountType,
                DiscountValue = voucher.DiscountValue,
                MinimumOrderAmount = voucher.MinimumOrderAmount,
                MaximumDiscountAmount = voucher.MaximumDiscountAmount,
                UsageLimit = voucher.UsageLimit,
                UsedCount = voucher.UsedCount,
                StartDate = voucher.StartDate,
                EndDate = voucher.EndDate,
                IsActive = voucher.IsActive,
                CreatedAt = voucher.CreatedAt,
                UpdatedAt = voucher.UpdatedAt
            };
        }
    }
}

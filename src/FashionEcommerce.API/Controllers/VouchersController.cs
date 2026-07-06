using FashionEcommerce.Services.Interfaces;
using FashionEcommerce.Services.Vouchers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VouchersController : ControllerBase
    {
        private readonly IVoucherService _voucherService;
        private readonly ILogger<VouchersController> _logger;

        public VouchersController(IVoucherService voucherService, ILogger<VouchersController> logger)
        {
            _voucherService = voucherService;
            _logger = logger;
        }

        /// <summary>
        /// Get all vouchers. Admin and staff only.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<IEnumerable<VoucherDto>>> GetAllVouchers([FromQuery] bool includeInactive = true)
        {
            try
            {
                var vouchers = await _voucherService.GetAllVouchersAsync(includeInactive);
                return Ok(vouchers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vouchers");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get voucher by id. Admin and staff only.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<VoucherDto>> GetVoucherById(int id)
        {
            try
            {
                var voucher = await _voucherService.GetVoucherByIdAsync(id);
                if (voucher == null)
                    return NotFound("Voucher not found");

                return Ok(voucher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting voucher {VoucherId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get voucher by code. Admin and staff only.
        /// </summary>
        [HttpGet("code/{code}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<VoucherDto>> GetVoucherByCode(string code)
        {
            try
            {
                var voucher = await _voucherService.GetVoucherByCodeAsync(code);
                if (voucher == null)
                    return NotFound("Voucher not found");

                return Ok(voucher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting voucher by code {VoucherCode}", code);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Validate a voucher code against an order amount.
        /// </summary>
        [HttpPost("validate")]
        public async Task<ActionResult<VoucherValidationResultDto>> ValidateVoucher([FromBody] ValidateVoucherRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required.");

                var result = await _voucherService.ValidateVoucherAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating voucher");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new voucher. Admin and staff only.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<VoucherDto>> CreateVoucher([FromBody] CreateVoucherDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Voucher cannot be null.");

                var result = await _voucherService.CreateVoucherAsync(dto);
                if (!result.Succeeded)
                    return ToErrorResponse(result);

                return CreatedAtAction(nameof(GetVoucherById), new { id = result.Data!.Id }, result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating voucher");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update an existing voucher. Admin and staff only.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<VoucherDto>> UpdateVoucher(int id, [FromBody] UpdateVoucherDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Voucher cannot be null.");

                var result = await _voucherService.UpdateVoucherAsync(id, dto);
                if (!result.Succeeded)
                    return ToErrorResponse(result);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating voucher {VoucherId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a voucher. Admin and staff only.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteVoucher(int id)
        {
            try
            {
                var deleted = await _voucherService.DeleteVoucherAsync(id);
                if (!deleted)
                    return NotFound("Voucher not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting voucher {VoucherId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        private ActionResult ToErrorResponse<T>(VoucherServiceResult<T> result)
        {
            return result.Error switch
            {
                VoucherServiceError.NotFound => NotFound(result.ErrorMessage),
                VoucherServiceError.Conflict => Conflict(result.ErrorMessage),
                VoucherServiceError.Validation => BadRequest(result.ErrorMessage),
                _ => StatusCode(500, result.ErrorMessage ?? "Internal server error")
            };
        }
    }
}

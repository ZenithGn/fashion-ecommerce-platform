using FashionEcommerce.Services.Interfaces;
using FashionEcommerce.Services.Marketing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")]
    public class MarketingController : ControllerBase
    {
        private readonly IMarketingService _marketingService;
        private readonly ILogger<MarketingController> _logger;

        public MarketingController(IMarketingService marketingService, ILogger<MarketingController> logger)
        {
            _marketingService = marketingService;
            _logger = logger;
        }

        /// <summary>
        /// Get marketing dashboard summary.
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<MarketingSummaryDto>> GetSummary()
        {
            try
            {
                var summary = await _marketingService.GetSummaryAsync();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting marketing summary");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get marketing campaigns with optional filters.
        /// </summary>
        [HttpGet("campaigns")]
        public async Task<ActionResult<IEnumerable<MarketingCampaignDto>>> GetCampaigns(
            [FromQuery] bool includeInactive = true,
            [FromQuery] FashionEcommerce.Core.Entities.MarketingCampaignStatus? status = null,
            [FromQuery] FashionEcommerce.Core.Entities.MarketingChannel? channel = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var campaigns = await _marketingService.GetCampaignsAsync(new MarketingCampaignQueryParameters
                {
                    IncludeInactive = includeInactive,
                    Status = status,
                    Channel = channel,
                    SearchTerm = searchTerm
                });

                return Ok(campaigns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting marketing campaigns");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get a marketing campaign by id.
        /// </summary>
        [HttpGet("campaigns/{id}")]
        public async Task<ActionResult<MarketingCampaignDto>> GetCampaignById(int id)
        {
            try
            {
                var campaign = await _marketingService.GetCampaignByIdAsync(id);
                if (campaign == null)
                    return NotFound("Campaign not found");

                return Ok(campaign);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting marketing campaign {CampaignId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a marketing campaign.
        /// </summary>
        [HttpPost("campaigns")]
        public async Task<ActionResult<MarketingCampaignDto>> CreateCampaign([FromBody] CreateMarketingCampaignDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Campaign cannot be null.");

                var result = await _marketingService.CreateCampaignAsync(dto);
                if (!result.Succeeded)
                    return ToErrorResponse(result);

                return CreatedAtAction(nameof(GetCampaignById), new { id = result.Data!.Id }, result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating marketing campaign");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update a marketing campaign.
        /// </summary>
        [HttpPut("campaigns/{id}")]
        public async Task<ActionResult<MarketingCampaignDto>> UpdateCampaign(int id, [FromBody] UpdateMarketingCampaignDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Campaign cannot be null.");

                var result = await _marketingService.UpdateCampaignAsync(id, dto);
                if (!result.Succeeded)
                    return ToErrorResponse(result);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating marketing campaign {CampaignId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update campaign status.
        /// </summary>
        [HttpPatch("campaigns/{id}/status")]
        public async Task<ActionResult<MarketingCampaignDto>> UpdateCampaignStatus(int id, [FromBody] UpdateMarketingCampaignStatusDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Status request cannot be null.");

                var result = await _marketingService.UpdateCampaignStatusAsync(id, dto.Status);
                if (!result.Succeeded)
                    return ToErrorResponse(result);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating marketing campaign status {CampaignId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update campaign performance metrics.
        /// </summary>
        [HttpPut("campaigns/{id}/metrics")]
        public async Task<ActionResult<MarketingCampaignDto>> UpdateCampaignMetrics(int id, [FromBody] UpdateMarketingCampaignMetricsDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Metrics request cannot be null.");

                var result = await _marketingService.UpdateCampaignMetricsAsync(id, dto);
                if (!result.Succeeded)
                    return ToErrorResponse(result);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating marketing campaign metrics {CampaignId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a marketing campaign.
        /// </summary>
        [HttpDelete("campaigns/{id}")]
        public async Task<IActionResult> DeleteCampaign(int id)
        {
            try
            {
                var deleted = await _marketingService.DeleteCampaignAsync(id);
                if (!deleted)
                    return NotFound("Campaign not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting marketing campaign {CampaignId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        private ActionResult ToErrorResponse<T>(MarketingServiceResult<T> result)
        {
            return result.Error switch
            {
                MarketingServiceError.NotFound => NotFound(result.ErrorMessage),
                MarketingServiceError.Conflict => Conflict(result.ErrorMessage),
                MarketingServiceError.Validation => BadRequest(result.ErrorMessage),
                _ => StatusCode(500, result.ErrorMessage ?? "Internal server error")
            };
        }
    }
}

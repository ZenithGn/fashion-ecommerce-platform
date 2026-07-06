using FashionEcommerce.Core.Entities;
using FashionEcommerce.Data;
using FashionEcommerce.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FashionEcommerce.Services.Marketing
{
    public class MarketingService : IMarketingService
    {
        private readonly FashionEcommerceDbContext _context;

        public MarketingService(FashionEcommerceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MarketingCampaignDto>> GetCampaignsAsync(MarketingCampaignQueryParameters parameters)
        {
            var query = _context.MarketingCampaigns
                .AsNoTracking()
                .Include(c => c.Voucher)
                .Where(c => !c.IsDeleted);

            if (!parameters.IncludeInactive)
                query = query.Where(c => c.IsActive);

            if (parameters.Status.HasValue)
                query = query.Where(c => c.Status == parameters.Status.Value);

            if (parameters.Channel.HasValue)
                query = query.Where(c => c.Channel == parameters.Channel.Value);

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                var searchTerm = parameters.SearchTerm.Trim();
                if (_context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
                {
                    var normalized = searchTerm.ToLowerInvariant();
                    query = query.Where(c =>
                        c.CampaignCode.ToLower().Contains(normalized) ||
                        c.Name.ToLower().Contains(normalized) ||
                        (c.Description != null && c.Description.ToLower().Contains(normalized)));
                }
                else
                {
                    var keyword = $"%{searchTerm}%";
                    query = query.Where(c =>
                        EF.Functions.ILike(c.CampaignCode, keyword) ||
                        EF.Functions.ILike(c.Name, keyword) ||
                        (c.Description != null && EF.Functions.ILike(c.Description, keyword)));
                }
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => ToDto(c))
                .ToListAsync();
        }

        public async Task<MarketingCampaignDto?> GetCampaignByIdAsync(int id)
        {
            return await _context.MarketingCampaigns
                .AsNoTracking()
                .Include(c => c.Voucher)
                .Where(c => c.Id == id && !c.IsDeleted)
                .Select(c => ToDto(c))
                .FirstOrDefaultAsync();
        }

        public async Task<MarketingServiceResult<MarketingCampaignDto>> CreateCampaignAsync(CreateMarketingCampaignDto dto)
        {
            var validationMessage = await ValidateCreateDtoAsync(dto);
            if (validationMessage != null)
                return MarketingServiceResult<MarketingCampaignDto>.Failure(MarketingServiceError.Validation, validationMessage);

            var normalizedCode = NormalizeCode(dto.CampaignCode);
            var codeExists = await _context.MarketingCampaigns.AnyAsync(c => c.CampaignCode == normalizedCode && !c.IsDeleted);
            if (codeExists)
                return MarketingServiceResult<MarketingCampaignDto>.Failure(MarketingServiceError.Conflict, "Campaign code already exists.");

            var campaign = new MarketingCampaign
            {
                CampaignCode = normalizedCode,
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                Channel = dto.Channel,
                Status = dto.Status,
                StartDate = EnsureUtc(dto.StartDate),
                EndDate = dto.EndDate.HasValue ? EnsureUtc(dto.EndDate.Value) : null,
                Budget = dto.Budget,
                ActualCost = dto.ActualCost,
                TargetAudience = dto.TargetAudience?.Trim(),
                Goal = dto.Goal?.Trim(),
                LandingPageUrl = dto.LandingPageUrl?.Trim(),
                VoucherId = dto.VoucherId,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.MarketingCampaigns.Add(campaign);
            await _context.SaveChangesAsync();

            var created = await GetCampaignByIdAsync(campaign.Id);
            return MarketingServiceResult<MarketingCampaignDto>.Success(created!);
        }

        public async Task<MarketingServiceResult<MarketingCampaignDto>> UpdateCampaignAsync(int id, UpdateMarketingCampaignDto dto)
        {
            var campaign = await _context.MarketingCampaigns.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (campaign == null)
                return MarketingServiceResult<MarketingCampaignDto>.Failure(MarketingServiceError.NotFound, "Campaign not found.");

            if (!string.IsNullOrWhiteSpace(dto.CampaignCode))
            {
                var normalizedCode = NormalizeCode(dto.CampaignCode);
                var codeExists = await _context.MarketingCampaigns.AnyAsync(c => c.Id != id && c.CampaignCode == normalizedCode && !c.IsDeleted);
                if (codeExists)
                    return MarketingServiceResult<MarketingCampaignDto>.Failure(MarketingServiceError.Conflict, "Campaign code already exists.");

                campaign.CampaignCode = normalizedCode;
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
                campaign.Name = dto.Name.Trim();

            if (dto.Description != null)
                campaign.Description = dto.Description.Trim();

            if (dto.Channel.HasValue)
                campaign.Channel = dto.Channel.Value;

            if (dto.Status.HasValue)
                campaign.Status = dto.Status.Value;

            if (dto.StartDate.HasValue)
                campaign.StartDate = EnsureUtc(dto.StartDate.Value);

            if (dto.EndDate.HasValue)
                campaign.EndDate = EnsureUtc(dto.EndDate.Value);

            if (dto.Budget.HasValue)
                campaign.Budget = dto.Budget.Value;

            if (dto.ActualCost.HasValue)
                campaign.ActualCost = dto.ActualCost.Value;

            if (dto.TargetAudience != null)
                campaign.TargetAudience = dto.TargetAudience.Trim();

            if (dto.Goal != null)
                campaign.Goal = dto.Goal.Trim();

            if (dto.LandingPageUrl != null)
                campaign.LandingPageUrl = dto.LandingPageUrl.Trim();

            if (dto.VoucherId.HasValue)
                campaign.VoucherId = dto.VoucherId.Value;

            if (dto.IsActive.HasValue)
                campaign.IsActive = dto.IsActive.Value;

            var validationMessage = await ValidateCampaignAsync(campaign);
            if (validationMessage != null)
                return MarketingServiceResult<MarketingCampaignDto>.Failure(MarketingServiceError.Validation, validationMessage);

            campaign.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var updated = await GetCampaignByIdAsync(campaign.Id);
            return MarketingServiceResult<MarketingCampaignDto>.Success(updated!);
        }

        public async Task<MarketingServiceResult<MarketingCampaignDto>> UpdateCampaignStatusAsync(int id, MarketingCampaignStatus status)
        {
            var campaign = await _context.MarketingCampaigns.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (campaign == null)
                return MarketingServiceResult<MarketingCampaignDto>.Failure(MarketingServiceError.NotFound, "Campaign not found.");

            campaign.Status = status;
            campaign.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var updated = await GetCampaignByIdAsync(campaign.Id);
            return MarketingServiceResult<MarketingCampaignDto>.Success(updated!);
        }

        public async Task<MarketingServiceResult<MarketingCampaignDto>> UpdateCampaignMetricsAsync(int id, UpdateMarketingCampaignMetricsDto dto)
        {
            var campaign = await _context.MarketingCampaigns.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (campaign == null)
                return MarketingServiceResult<MarketingCampaignDto>.Failure(MarketingServiceError.NotFound, "Campaign not found.");

            if (dto.Impressions < 0 || dto.Clicks < 0 || dto.Conversions < 0 || dto.ActualCost < 0 || dto.Revenue < 0)
                return MarketingServiceResult<MarketingCampaignDto>.Failure(MarketingServiceError.Validation, "Metrics cannot be negative.");

            if (dto.Clicks > dto.Impressions)
                return MarketingServiceResult<MarketingCampaignDto>.Failure(MarketingServiceError.Validation, "Clicks cannot be greater than impressions.");

            if (dto.Conversions > dto.Clicks)
                return MarketingServiceResult<MarketingCampaignDto>.Failure(MarketingServiceError.Validation, "Conversions cannot be greater than clicks.");

            campaign.Impressions = dto.Impressions;
            campaign.Clicks = dto.Clicks;
            campaign.Conversions = dto.Conversions;
            campaign.ActualCost = dto.ActualCost;
            campaign.Revenue = dto.Revenue;
            campaign.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var updated = await GetCampaignByIdAsync(campaign.Id);
            return MarketingServiceResult<MarketingCampaignDto>.Success(updated!);
        }

        public async Task<bool> DeleteCampaignAsync(int id)
        {
            var campaign = await _context.MarketingCampaigns.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (campaign == null)
                return false;

            campaign.IsDeleted = true;
            campaign.IsActive = false;
            campaign.Status = MarketingCampaignStatus.Cancelled;
            campaign.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<MarketingSummaryDto> GetSummaryAsync()
        {
            var campaigns = await _context.MarketingCampaigns
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            var totalImpressions = campaigns.Sum(c => c.Impressions);
            var totalClicks = campaigns.Sum(c => c.Clicks);
            var totalConversions = campaigns.Sum(c => c.Conversions);
            var totalActualCost = campaigns.Sum(c => c.ActualCost);
            var totalRevenue = campaigns.Sum(c => c.Revenue);

            return new MarketingSummaryDto
            {
                TotalCampaigns = campaigns.Count,
                ActiveCampaigns = campaigns.Count(c => c.IsActive),
                RunningCampaigns = campaigns.Count(c => c.Status == MarketingCampaignStatus.Running),
                TotalBudget = campaigns.Sum(c => c.Budget),
                TotalActualCost = totalActualCost,
                TotalRevenue = totalRevenue,
                TotalImpressions = totalImpressions,
                TotalClicks = totalClicks,
                TotalConversions = totalConversions,
                AverageClickThroughRate = totalImpressions == 0 ? 0 : Math.Round((decimal)totalClicks / totalImpressions * 100, 2),
                AverageConversionRate = totalClicks == 0 ? 0 : Math.Round((decimal)totalConversions / totalClicks * 100, 2),
                OverallReturnOnInvestment = totalActualCost == 0 ? 0 : Math.Round((totalRevenue - totalActualCost) / totalActualCost * 100, 2)
            };
        }

        private async Task<string?> ValidateCreateDtoAsync(CreateMarketingCampaignDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CampaignCode))
                return "CampaignCode is required.";

            if (string.IsNullOrWhiteSpace(dto.Name))
                return "Name is required.";

            var campaign = new MarketingCampaign
            {
                CampaignCode = NormalizeCode(dto.CampaignCode),
                Name = dto.Name.Trim(),
                StartDate = EnsureUtc(dto.StartDate),
                EndDate = dto.EndDate.HasValue ? EnsureUtc(dto.EndDate.Value) : null,
                Budget = dto.Budget,
                ActualCost = dto.ActualCost,
                VoucherId = dto.VoucherId
            };

            return await ValidateCampaignAsync(campaign);
        }

        private async Task<string?> ValidateCampaignAsync(MarketingCampaign campaign)
        {
            if (campaign.Budget < 0)
                return "Budget cannot be negative.";

            if (campaign.ActualCost < 0)
                return "ActualCost cannot be negative.";

            if (campaign.EndDate.HasValue && campaign.EndDate.Value <= campaign.StartDate)
                return "EndDate must be after StartDate.";

            if (campaign.VoucherId.HasValue)
            {
                var voucherExists = await _context.Vouchers.AnyAsync(v => v.Id == campaign.VoucherId.Value && !v.IsDeleted);
                if (!voucherExists)
                    return "VoucherId is invalid.";
            }

            return null;
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

        private static MarketingCampaignDto ToDto(MarketingCampaign campaign)
        {
            return new MarketingCampaignDto
            {
                Id = campaign.Id,
                CampaignCode = campaign.CampaignCode,
                Name = campaign.Name,
                Description = campaign.Description,
                Channel = campaign.Channel,
                Status = campaign.Status,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                Budget = campaign.Budget,
                ActualCost = campaign.ActualCost,
                TargetAudience = campaign.TargetAudience,
                Goal = campaign.Goal,
                LandingPageUrl = campaign.LandingPageUrl,
                VoucherId = campaign.VoucherId,
                VoucherCode = campaign.Voucher?.Code,
                Impressions = campaign.Impressions,
                Clicks = campaign.Clicks,
                Conversions = campaign.Conversions,
                Revenue = campaign.Revenue,
                ClickThroughRate = campaign.Impressions == 0 ? 0 : Math.Round((decimal)campaign.Clicks / campaign.Impressions * 100, 2),
                ConversionRate = campaign.Clicks == 0 ? 0 : Math.Round((decimal)campaign.Conversions / campaign.Clicks * 100, 2),
                ReturnOnInvestment = campaign.ActualCost == 0 ? 0 : Math.Round((campaign.Revenue - campaign.ActualCost) / campaign.ActualCost * 100, 2),
                IsActive = campaign.IsActive,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt
            };
        }
    }
}

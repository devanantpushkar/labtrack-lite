using LabTrackApi.Models;
using LabTrackApi.Data;
using LabTrackApi.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LabTrackApi.Services
{
    public class AssetService
    {
        private readonly AppDbContext _context;

        public AssetService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResponse<AssetResponse>> GetAssetsAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? status = null, 
            string? category = null,
            string? search = null)
        {
            var query = _context.Assets
                .Include(a => a.Creator)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<AssetStatus>(status, true, out var statusEnum))
                {
                    query = query.Where(a => a.Status == statusEnum);
                }
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(a => a.Category == category);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a => 
                    a.Name.Contains(search) || 
                    (a.Description != null && a.Description.Contains(search)) ||
                    (a.QRCode != null && a.QRCode.Contains(search)));
            }

            var totalCount = await query.CountAsync();

            var assets = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AssetResponse
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    QRCode = a.QRCode,
                    Status = a.Status.ToString(),
                    Location = a.Location,
                    Category = a.Category,
                    CreatedBy = a.CreatedBy,
                    CreatorName = a.Creator != null ? a.Creator.Username : null,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PaginatedResponse<AssetResponse>
            {
                Items = assets,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages
            };
        }

        public async Task<AssetResponse?> GetAssetByIdAsync(int id)
        {
            var asset = await _context.Assets
                .Include(a => a.Creator)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asset == null) return null;

            return new AssetResponse
            {
                Id = asset.Id,
                Name = asset.Name,
                Description = asset.Description,
                QRCode = asset.QRCode,
                Status = asset.Status.ToString(),
                Location = asset.Location,
                Category = asset.Category,
                CreatedBy = asset.CreatedBy,
                CreatorName = asset.Creator?.Username,
                CreatedAt = asset.CreatedAt,
                UpdatedAt = asset.UpdatedAt
            };
        }

        public async Task<AssetResponse> CreateAssetAsync(CreateAssetRequest request, int userId)
        {
            var qrCode = request.QRCode ?? $"ASSET-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";

            var asset = new Asset
            {
                Name = request.Name,
                Description = request.Description,
                QRCode = qrCode,
                Status = request.Status,
                Location = request.Location,
                Category = request.Category,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();

            await LogAuditAsync(userId, "Create", "Asset", asset.Id);

            return new AssetResponse
            {
                Id = asset.Id,
                Name = asset.Name,
                Description = asset.Description,
                QRCode = asset.QRCode,
                Status = asset.Status.ToString(),
                Location = asset.Location,
                Category = asset.Category,
                CreatedBy = asset.CreatedBy,
                CreatedAt = asset.CreatedAt,
                UpdatedAt = asset.UpdatedAt
            };
        }

        public async Task<AssetResponse?> UpdateAssetAsync(int id, UpdateAssetRequest request, int userId)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return null;

            if (!string.IsNullOrEmpty(request.Name))
                asset.Name = request.Name;

            if (request.Description != null)
                asset.Description = request.Description;

            if (request.QRCode != null)
                asset.QRCode = request.QRCode;

            if (request.Status.HasValue)
                asset.Status = request.Status.Value;

            if (request.Location != null)
                asset.Location = request.Location;

            if (request.Category != null)
                asset.Category = request.Category;

            asset.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await LogAuditAsync(userId, "Update", "Asset", asset.Id);

            return await GetAssetByIdAsync(id);
        }

        public async Task<bool> DeleteAssetAsync(int id, int userId)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return false;

            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();

            await LogAuditAsync(userId, "Delete", "Asset", id);

            return true;
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _context.Assets
                .Where(a => a.Category != null)
                .Select(a => a.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        private async Task LogAuditAsync(int userId, string action, string entityType, int entityId)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}

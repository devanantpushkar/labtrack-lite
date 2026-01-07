using System.Security.Claims;
using LabTrackApi.DTOs;
using LabTrackApi.Services;

namespace LabTrackApi.Endpoints
{
    public static class AssetEndpoints
    {
        public static void MapAssetEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/assets").WithTags("Assets").RequireAuthorization();

            group.MapGet("/", async (
                AssetService assetService,
                int pageNumber = 1,
                int pageSize = 10,
                string? status = null,
                string? category = null,
                string? search = null) =>
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var result = await assetService.GetAssetsAsync(pageNumber, pageSize, status, category, search);
                return Results.Ok(result);
            })
            .WithName("GetAssets")
            .WithOpenApi();

            group.MapGet("/{id:int}", async (int id, AssetService assetService) =>
            {
                var asset = await assetService.GetAssetByIdAsync(id);
                if (asset == null)
                {
                    return Results.NotFound(new { Message = "Asset not found" });
                }
                return Results.Ok(asset);
            })
            .WithName("GetAssetById")
            .WithOpenApi();

            group.MapPost("/", async (
                CreateAssetRequest request,
                AssetService assetService,
                HttpContext context) =>
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Results.Unauthorized();
                }

                var userId = int.Parse(userIdClaim.Value);

                if (string.IsNullOrEmpty(request.Name))
                {
                    return Results.BadRequest(new { Message = "Asset name is required" });
                }

                var asset = await assetService.CreateAssetAsync(request, userId);
                return Results.Created($"/api/assets/{asset.Id}", asset);
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Engineer"))
            .WithName("CreateAsset")
            .WithOpenApi();

            group.MapPut("/{id:int}", async (
                int id,
                UpdateAssetRequest request,
                AssetService assetService,
                HttpContext context) =>
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Results.Unauthorized();
                }

                var userId = int.Parse(userIdClaim.Value);

                var asset = await assetService.UpdateAssetAsync(id, request, userId);
                if (asset == null)
                {
                    return Results.NotFound(new { Message = "Asset not found" });
                }
                return Results.Ok(asset);
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Engineer"))
            .WithName("UpdateAsset")
            .WithOpenApi();

            group.MapDelete("/{id:int}", async (
                int id,
                AssetService assetService,
                HttpContext context) =>
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Results.Unauthorized();
                }

                var userId = int.Parse(userIdClaim.Value);

                var deleted = await assetService.DeleteAssetAsync(id, userId);
                if (!deleted)
                {
                    return Results.NotFound(new { Message = "Asset not found" });
                }
                return Results.NoContent();
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("DeleteAsset")
            .WithOpenApi();

            group.MapGet("/categories", async (AssetService assetService) =>
            {
                var categories = await assetService.GetCategoriesAsync();
                return Results.Ok(categories);
            })
            .WithName("GetAssetCategories")
            .WithOpenApi();
        }
    }
}

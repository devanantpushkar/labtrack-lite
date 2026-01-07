using LabTrackApi.DTOs;
using LabTrackApi.Services;
using System.Security.Claims;

namespace LabTrackApi.Endpoints
{
    public static class ChatbotEndpoints
    {
        public static void MapChatbotEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/chatbot").WithTags("Chatbot").RequireAuthorization();

            group.MapPost("/query", async (ChatbotRequest request, ChatbotService chatbotService, ClaimsPrincipal user) =>
            {
                if (string.IsNullOrEmpty(request.Query))
                {
                    return Results.BadRequest(new { Message = "Query is required" });
                }

                var sanitizedQuery = request.Query
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Trim();

                if (sanitizedQuery.Length > 500)
                {
                    return Results.BadRequest(new { Message = "Query is too long (max 500 characters)" });
                }

                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                var roleClaim = user.FindFirst(ClaimTypes.Role);
                var userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
                var role = roleClaim?.Value ?? "";

                var response = await chatbotService.ProcessQueryAsync(sanitizedQuery, userId, role);
                return Results.Ok(response);
            })
            .WithName("ProcessChatbotQuery")
            .WithOpenApi();

            group.MapGet("/help", () =>
            {
                var helpResponse = new
                {
                    Message = "LabTrack Chatbot Help",
                    ExampleQueries = new[]
                    {
                        "How many assets are there?",
                        "List all open tickets",
                        "Show available assets",
                        "Count tickets in progress",
                        "What is the status of assets?",
                        "Show critical tickets",
                        "List assets in maintenance",
                        "Who is assigned to ticket 1?",
                        "Show unassigned tickets"
                    },
                    SupportedOperations = new[]
                    {
                        "Count assets/tickets",
                        "List assets/tickets with filters",
                        "Get status summaries",
                        "Find assigned/unassigned tickets",
                        "Search by name or status"
                    }
                };

                return Results.Ok(helpResponse);
            })
            .WithName("GetChatbotHelp")
            .WithOpenApi();
        }
    }
}

using System.Security.Claims;
using LabTrackApi.DTOs;
using LabTrackApi.Services;

namespace LabTrackApi.Endpoints
{
    public static class TicketEndpoints
    {
        public static void MapTicketEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/tickets").WithTags("Tickets").RequireAuthorization();

            group.MapGet("/", async (
                TicketService ticketService,
                HttpContext context,
                int pageNumber = 1,
                int pageSize = 10,
                string? status = null,
                string? priority = null,
                int? assetId = null) =>
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                var roleClaim = context.User.FindFirst(ClaimTypes.Role);

                var userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
                var role = roleClaim?.Value ?? "";

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var ownOnly = role != "Admin";

                var result = await ticketService.GetTicketsAsync(
                    pageNumber, pageSize, status, priority, assetId, userId, ownOnly);
                return Results.Ok(result);
            })
            .WithName("GetTickets")
            .WithOpenApi();

            group.MapGet("/{id:int}", async (int id, TicketService ticketService, HttpContext context) =>
            {
                var ticket = await ticketService.GetTicketByIdAsync(id);
                if (ticket == null)
                {
                    return Results.NotFound(new { Message = "Ticket not found" });
                }

                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                var roleClaim = context.User.FindFirst(ClaimTypes.Role);
                var userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
                var role = roleClaim?.Value ?? "";

                if (role != "Admin" && ticket.CreatedBy != userId && ticket.AssignedTo != userId)
                {
                    return Results.Forbid();
                }

                return Results.Ok(ticket);
            })
            .WithName("GetTicketById")
            .WithOpenApi();

            group.MapPost("/", async (
                CreateTicketRequest request,
                TicketService ticketService,
                HttpContext context) =>
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Results.Unauthorized();
                }

                var userId = int.Parse(userIdClaim.Value);

                if (string.IsNullOrEmpty(request.Title))
                {
                    return Results.BadRequest(new { Message = "Ticket title is required" });
                }

                var ticket = await ticketService.CreateTicketAsync(request, userId);
                return Results.Created($"/api/tickets/{ticket.Id}", ticket);
            })
            .WithName("CreateTicket")
            .WithOpenApi();

            group.MapPut("/{id:int}", async (
                int id,
                UpdateTicketRequest request,
                TicketService ticketService,
                HttpContext context) =>
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                var roleClaim = context.User.FindFirst(ClaimTypes.Role);

                if (userIdClaim == null)
                {
                    return Results.Unauthorized();
                }

                var userId = int.Parse(userIdClaim.Value);
                var role = roleClaim?.Value ?? "";

                var existingTicket = await ticketService.GetTicketByIdAsync(id);
                if (existingTicket == null)
                {
                    return Results.NotFound(new { Message = "Ticket not found" });
                }

                if (role != "Admin" && existingTicket.CreatedBy != userId && existingTicket.AssignedTo != userId)
                {
                    return Results.Forbid();
                }

                if (request.AssignedTo.HasValue && role != "Admin")
                {
                    return Results.Forbid();
                }

                var ticket = await ticketService.UpdateTicketAsync(id, request, userId);
                if (ticket == null)
                {
                    return Results.BadRequest(new { Message = "Invalid status transition" });
                }
                return Results.Ok(ticket);
            })
            .WithName("UpdateTicket")
            .WithOpenApi();

            group.MapDelete("/{id:int}", async (
                int id,
                TicketService ticketService,
                HttpContext context) =>
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Results.Unauthorized();
                }

                var userId = int.Parse(userIdClaim.Value);

                var deleted = await ticketService.DeleteTicketAsync(id, userId);
                if (!deleted)
                {
                    return Results.NotFound(new { Message = "Ticket not found" });
                }
                return Results.NoContent();
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("DeleteTicket")
            .WithOpenApi();

            group.MapPost("/{ticketId:int}/comments", async (
                int ticketId,
                CreateCommentRequest request,
                TicketService ticketService,
                HttpContext context) =>
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Results.Unauthorized();
                }

                var userId = int.Parse(userIdClaim.Value);

                if (string.IsNullOrEmpty(request.Content))
                {
                    return Results.BadRequest(new { Message = "Comment content is required" });
                }

                var comment = await ticketService.AddCommentAsync(ticketId, request, userId);
                if (comment == null)
                {
                    return Results.NotFound(new { Message = "Ticket not found" });
                }
                return Results.Created($"/api/tickets/{ticketId}/comments/{comment.Id}", comment);
            })
            .WithName("AddComment")
            .WithOpenApi();

            group.MapDelete("/{ticketId:int}/comments/{commentId:int}", async (
                int ticketId,
                int commentId,
                TicketService ticketService,
                HttpContext context) =>
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                var roleClaim = context.User.FindFirst(ClaimTypes.Role);

                if (userIdClaim == null)
                {
                    return Results.Unauthorized();
                }

                var userId = int.Parse(userIdClaim.Value);
                var isAdmin = roleClaim?.Value == "Admin";

                var deleted = await ticketService.DeleteCommentAsync(commentId, userId, isAdmin);
                if (!deleted)
                {
                    return Results.NotFound(new { Message = "Comment not found or not authorized" });
                }
                return Results.NoContent();
            })
            .WithName("DeleteComment")
            .WithOpenApi();
        }
    }
}

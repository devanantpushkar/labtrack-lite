using LabTrackApi.Models;
using LabTrackApi.Data;
using LabTrackApi.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LabTrackApi.Services
{
    public class TicketService
    {
        private readonly AppDbContext _context;

        public TicketService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResponse<TicketResponse>> GetTicketsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? status = null,
            string? priority = null,
            int? assetId = null,
            int? userId = null,
            bool ownOnly = false)
        {
            var query = _context.Tickets
                .Include(t => t.Asset)
                .Include(t => t.Creator)
                .Include(t => t.Assignee)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<TicketStatus>(status, true, out var statusEnum))
                {
                    query = query.Where(t => t.Status == statusEnum);
                }
            }

            if (!string.IsNullOrEmpty(priority))
            {
                if (Enum.TryParse<TicketPriority>(priority, true, out var priorityEnum))
                {
                    query = query.Where(t => t.Priority == priorityEnum);
                }
            }

            if (assetId.HasValue)
            {
                query = query.Where(t => t.AssetId == assetId.Value);
            }

            if (ownOnly && userId.HasValue)
            {
                query = query.Where(t => t.CreatedBy == userId.Value || t.AssignedTo == userId.Value);
            }

            var totalCount = await query.CountAsync();

            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TicketResponse
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status.ToString(),
                    Priority = t.Priority.ToString(),
                    AssetId = t.AssetId,
                    AssetName = t.Asset != null ? t.Asset.Name : null,
                    CreatedBy = t.CreatedBy,
                    CreatorName = t.Creator != null ? t.Creator.Username : null,
                    AssignedTo = t.AssignedTo,
                    AssigneeName = t.Assignee != null ? t.Assignee.Username : null,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PaginatedResponse<TicketResponse>
            {
                Items = tickets,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages
            };
        }

        public async Task<TicketResponse?> GetTicketByIdAsync(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Asset)
                .Include(t => t.Creator)
                .Include(t => t.Assignee)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return null;

            return new TicketResponse
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status.ToString(),
                Priority = ticket.Priority.ToString(),
                AssetId = ticket.AssetId,
                AssetName = ticket.Asset?.Name,
                CreatedBy = ticket.CreatedBy,
                CreatorName = ticket.Creator?.Username,
                AssignedTo = ticket.AssignedTo,
                AssigneeName = ticket.Assignee?.Username,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                Comments = ticket.Comments
                    .OrderBy(c => c.CreatedAt)
                    .Select(c => new CommentResponse
                    {
                        Id = c.Id,
                        TicketId = c.TicketId,
                        UserId = c.UserId,
                        Username = c.User?.Username,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt
                    })
                    .ToList()
            };
        }

        public async Task<TicketResponse> CreateTicketAsync(CreateTicketRequest request, int userId)
        {
            var ticket = new Ticket
            {
                Title = request.Title,
                Description = request.Description,
                Status = TicketStatus.Open,
                Priority = request.Priority,
                AssetId = request.AssetId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            await LogAuditAsync(userId, "Create", "Ticket", ticket.Id);

            return (await GetTicketByIdAsync(ticket.Id))!;
        }

        public async Task<TicketResponse?> UpdateTicketAsync(int id, UpdateTicketRequest request, int userId)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return null;

            if (request.Status.HasValue)
            {
                if (!IsValidStatusTransition(ticket.Status, request.Status.Value))
                {
                    return null; // Invalid transition
                }
                ticket.Status = request.Status.Value;
            }

            if (!string.IsNullOrEmpty(request.Title))
                ticket.Title = request.Title;

            if (request.Description != null)
                ticket.Description = request.Description;

            if (request.Priority.HasValue)
                ticket.Priority = request.Priority.Value;

            if (request.AssignedTo.HasValue)
                ticket.AssignedTo = request.AssignedTo.Value;

            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await LogAuditAsync(userId, "Update", "Ticket", ticket.Id);

            return await GetTicketByIdAsync(id);
        }

        public async Task<bool> DeleteTicketAsync(int id, int userId)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return false;

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            await LogAuditAsync(userId, "Delete", "Ticket", id);

            return true;
        }

        public async Task<CommentResponse?> AddCommentAsync(int ticketId, CreateCommentRequest request, int userId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return null;

            var comment = new Comment
            {
                TicketId = ticketId,
                UserId = userId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

            return new CommentResponse
            {
                Id = comment.Id,
                TicketId = comment.TicketId,
                UserId = comment.UserId,
                Username = user?.Username,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<bool> DeleteCommentAsync(int commentId, int userId, bool isAdmin)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null) return false;

            if (comment.UserId != userId && !isAdmin) return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return true;
        }

        private bool IsValidStatusTransition(TicketStatus current, TicketStatus next)
        {
            var validTransitions = new Dictionary<TicketStatus, List<TicketStatus>>
            {
                { TicketStatus.Open, new List<TicketStatus> { TicketStatus.InProgress, TicketStatus.Closed } },
                { TicketStatus.InProgress, new List<TicketStatus> { TicketStatus.Open, TicketStatus.Resolved, TicketStatus.Closed } },
                { TicketStatus.Resolved, new List<TicketStatus> { TicketStatus.InProgress, TicketStatus.Closed } },
                { TicketStatus.Closed, new List<TicketStatus> { TicketStatus.Open } } // Reopen
            };

            if (validTransitions.TryGetValue(current, out var allowed))
            {
                return allowed.Contains(next);
            }

            return false;
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

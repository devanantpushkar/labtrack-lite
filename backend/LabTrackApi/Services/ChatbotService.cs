using LabTrackApi.Data;
using LabTrackApi.DTOs;
using LabTrackApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace LabTrackApi.Services
{
    public class ChatbotService
    {
        private readonly AppDbContext _context;

        public ChatbotService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ChatbotResponse> ProcessQueryAsync(string query, int userId, string role)
        {
            query = query.ToLower().Trim();

            if (query.Contains("how many assets") || query.Contains("count assets"))
            {
                var count = await _context.Assets.CountAsync();
                return new ChatbotResponse 
                { 
                    Message = $"There are currently {count} total assets in the system.",
                    QueryType = "count_assets" 
                };
            }

            if (query.Contains("how many tickets") || query.Contains("count tickets"))
            {
                var totalCount = await _context.Tickets.CountAsync();
                
                if (role == "Admin")
                {
                    return new ChatbotResponse 
                    { 
                        Message = $"There are currently {totalCount} total tickets in the system.",
                        QueryType = "count_tickets" 
                    };
                }
                else
                {
                    var userCount = await _context.Tickets.CountAsync(t => t.CreatedBy == userId || t.AssignedTo == userId);
                    return new ChatbotResponse 
                    { 
                        Message = $"You have access to {userCount} tickets (System Total: {totalCount}).",
                        QueryType = "count_tickets" 
                    };
                }
            }

            if (query.Contains("available assets") || query.Contains("what is available"))
            {
                var assets = await _context.Assets
                    .Where(a => a.Status == AssetStatus.Available)
                    .Take(5)
                    .ToListAsync();
                
                var names = string.Join(", ", assets.Select(a => a.Name));
                return new ChatbotResponse 
                { 
                    Message = assets.Any() 
                        ? $"Here are some available assets: {names}" 
                        : "No assets are currently available.",
                    QueryType = "list_assets"
                };
            }

            var ticketMatch = Regex.Match(query, @"ticket\s*(?:status|id)?\s*#?(\d+)");
            if (ticketMatch.Success)
            {
                if (int.TryParse(ticketMatch.Groups[1].Value, out int ticketId))
                {
                    var ticket = await _context.Tickets
                        .Include(t => t.Asset)
                        .FirstOrDefaultAsync(t => t.Id == ticketId);

                    if (ticket != null)
                    {
                        return new ChatbotResponse 
                        { 
                            Message = $"Ticket #{ticket.Id} for {ticket.Asset?.Name} is currently '{ticket.Status}' with '{ticket.Priority}' priority.",
                            QueryType = "ticket_status"
                        };
                    }
                    else
                    {
                        return new ChatbotResponse { Message = $"I couldn't find a ticket with ID #{ticketId}.", QueryType = "not_found" };
                    }
                }
            }

            return new ChatbotResponse 
            { 
                Message = "I'm sorry, I didn't understand that. You can ask me:\n- 'How many assets?'\n- 'Show available assets'\n- 'Status of ticket #1'",
                QueryType = "unknown"
            };
        }
    }
}

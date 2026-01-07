using System.Security.Claims;
using LabTrackApi.DTOs;
using LabTrackApi.Services;

namespace LabTrackApi.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/auth").WithTags("Authentication");

            group.MapPost("/register", async (RegisterRequest request, AuthService authService) =>
            {
                Console.WriteLine($"Registering user: {request.Username}");
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return Results.BadRequest(new { Message = "Username and password are required" });
                }

                if (request.Password.Length < 6)
                {
                    return Results.BadRequest(new { Message = "Password must be at least 6 characters" });
                }

                try 
                {
                    var result = await authService.RegisterAsync(request);
                    if (result == null)
                    {
                        Console.WriteLine($"Registration failed for '{request.Username}': Username or email already exists.");
                        return Results.BadRequest(new { Message = "Username or email already exists" });
                    }

                    Console.WriteLine($"User '{request.Username}' registered successfully.");
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CRITICAL ERROR during registration for '{request.Username}': {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                    return Results.Problem("Internal server error during registration.");
                }
            })
            .RequireRateLimiting("AuthPolicy")
            .WithName("Register")
            .WithOpenApi();

            group.MapPost("/login", async (LoginRequest request, AuthService authService) =>
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return Results.BadRequest(new { Message = "Username and password are required" });
                }

                var result = await authService.LoginAsync(request);
                if (result == null)
                {
                    return Results.Unauthorized();
                }

                return Results.Ok(result);
            })
            .RequireRateLimiting("AuthPolicy")
            .WithName("Login")
            .WithOpenApi();

            group.MapGet("/me", async (HttpContext context, AuthService authService) =>
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Results.Unauthorized();
                }

                var userId = int.Parse(userIdClaim.Value);
                var user = await authService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(user);
            })
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .WithOpenApi();

            group.MapGet("/users", async (AuthService authService) =>
            {
                var users = await authService.GetAllUsersAsync();
                return Results.Ok(users);
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("GetAllUsers")
            .WithOpenApi();
        }
    }
}

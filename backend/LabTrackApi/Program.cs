using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using LabTrackApi.Data;
using LabTrackApi.Services;
using LabTrackApi.Endpoints;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var rawConnectionString = (builder.Configuration.GetConnectionString("PostgreSQL") ?? Environment.GetEnvironmentVariable("DATABASE_URL"))?.Trim();
string? pgConnectionString = null;

if (!string.IsNullOrWhiteSpace(rawConnectionString))
{
    Console.WriteLine($"Database connection string detected (Length: {rawConnectionString.Length})");
    
    if (rawConnectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) || 
        rawConnectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        try 
        {
            Console.WriteLine("Detected PostgreSQL URI format. Converting to ADO.NET format...");
            var uri = new Uri(rawConnectionString);
            var host = uri.Host;
            var dbPort = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');
            var userInfo = uri.UserInfo.Split(':');
            var user = userInfo[0];
            var password = userInfo.Length > 1 ? userInfo[1] : "";
            
            pgConnectionString = $"Host={host};Port={dbPort};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true";
            Console.WriteLine("Successfully converted URI to ADO.NET format.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CRITICAL ERROR: Failed to parse PostgreSQL URI. {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("Using connection string as-is (ADO.NET format).");
        pgConnectionString = rawConnectionString;
    }
}
else
{
    Console.WriteLine("No database connection string found in DATABASE_URL or PostgreSQL ConnectionString.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(pgConnectionString))
    {
        // One final check: if it still starts with postgres://, npgsql WILL crash.
        if (pgConnectionString.StartsWith("postgres", StringComparison.OrdinalIgnoreCase))
        {
             Console.WriteLine("WARNING: Connection string still starts with 'postgres'. Npgsql will likely fail.");
        }
        
        options.UseNpgsql(pgConnectionString);
    }
    else
    {
        Console.WriteLine("Falling back to SQLite (labtrack.db).");
        options.UseSqlite("Data Source=labtrack.db");
    }
});

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AssetService>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<ChatbotService>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing in configuration.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LabTrackApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "LabTrackApp";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter(policyName: "AuthPolicy", options =>
    {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromSeconds(30);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LabTrack API",
        Version = "v1",
        Description = "R&D Asset & Ticketing Platform API"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseCors("AllowReactApp");
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "LabTrack API v1");
    });
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapAssetEndpoints();
app.MapTicketEndpoints();
app.MapChatbotEndpoints();

app.MapGet("/api/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
   .WithTags("Health");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    if (!context.Users.Any())
    {
        var adminUser = new LabTrackApi.Models.User
        {
            Username = "admin",
            Email = "admin@labtrack.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = LabTrackApi.Models.UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(adminUser);

        var engineerUser = new LabTrackApi.Models.User
        {
            Username = "engineer",
            Email = "engineer@labtrack.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("engineer123"),
            Role = LabTrackApi.Models.UserRole.Engineer,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(engineerUser);

        var technicianUser = new LabTrackApi.Models.User
        {
            Username = "technician",
            Email = "technician@labtrack.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("tech123"),
            Role = LabTrackApi.Models.UserRole.Technician,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(technicianUser);

        context.SaveChanges();

        var assets = new[]
        {
            new LabTrackApi.Models.Asset
            {
                Name = "Microscope A1",
                Description = "High-powered optical microscope for lab research",
                QRCode = "ASSET-MICRO001",
                Status = LabTrackApi.Models.AssetStatus.Available,
                Location = "Lab Room 101",
                Category = "Equipment",
                CreatedBy = adminUser.Id
            },
            new LabTrackApi.Models.Asset
            {
                Name = "Centrifuge B2",
                Description = "High-speed centrifuge for sample processing",
                QRCode = "ASSET-CENTR002",
                Status = LabTrackApi.Models.AssetStatus.InUse,
                Location = "Lab Room 102",
                Category = "Equipment",
                CreatedBy = adminUser.Id
            },
            new LabTrackApi.Models.Asset
            {
                Name = "Spectrometer S1",
                Description = "UV-Vis spectrometer for chemical analysis",
                QRCode = "ASSET-SPECT003",
                Status = LabTrackApi.Models.AssetStatus.Maintenance,
                Location = "Lab Room 103",
                Category = "Instruments",
                CreatedBy = adminUser.Id
            }
        };
        context.Assets.AddRange(assets);
        context.SaveChanges();

        var tickets = new[]
        {
            new LabTrackApi.Models.Ticket
            {
                Title = "Microscope calibration needed",
                Description = "The microscope A1 needs recalibration after recent maintenance",
                Status = LabTrackApi.Models.TicketStatus.Open,
                Priority = LabTrackApi.Models.TicketPriority.Medium,
                AssetId = assets[0].Id,
                CreatedBy = technicianUser.Id
            },
            new LabTrackApi.Models.Ticket
            {
                Title = "Centrifuge making unusual noise",
                Description = "The centrifuge B2 is making a grinding noise during operation",
                Status = LabTrackApi.Models.TicketStatus.InProgress,
                Priority = LabTrackApi.Models.TicketPriority.High,
                AssetId = assets[1].Id,
                CreatedBy = engineerUser.Id,
                AssignedTo = engineerUser.Id
            }
        };
        context.Tickets.AddRange(tickets);
        context.SaveChanges();
    }
}

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");

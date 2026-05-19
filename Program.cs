using FluentValidation;
using FluentValidation.AspNetCore;
using MetroSol.API.Services;
using MetroSol.Core.Interfaces;
using MetroSol.Infrastructure.Data;
using MetroSol.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<MetroSolDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ITokenService, TokenService>();

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtKey    = builder.Configuration["Jwt:Secret"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAud    = builder.Configuration["Jwt:Audience"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            ValidAudience            = jwtAud,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew                = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ── Rate Limiting ─────────────────────────────────────────────────────────────
// "auth"   → 10 req/min per IP  (login, register, refresh — brute-force protection)
// "api"    → 200 req/min per IP (all other authenticated endpoints)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            """{"message":"Too many requests. Please slow down and try again later."}""",
            cancellationToken);
    };

    // Strict policy for authentication endpoints
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.Window               = TimeSpan.FromMinutes(1);
        opt.PermitLimit          = 10;
        opt.QueueLimit           = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // General policy for all API endpoints
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.Window               = TimeSpan.FromMinutes(1);
        opt.PermitLimit          = 200;
        opt.QueueLimit           = 5;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

// ── FluentValidation ──────────────────────────────────────────────────────────
builder.Services
    .AddFluentValidationAutoValidation()
    .AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "MetroSol API";
        options.Theme = ScalarTheme.DeepSpace;
    });
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("api");

app.Run();

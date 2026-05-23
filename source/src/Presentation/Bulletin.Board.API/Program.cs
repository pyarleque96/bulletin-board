using System.Text;
using Asp.Versioning;
using Bulletin.Board.Application;
using Bulletin.Board.Application.Snapshots;
using Bulletin.Board.Infrastructure;
using Bulletin.Board.Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console());

// Application + Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Controllers with Problem Details
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// JWT Authentication
// La clave DEBE estar en JWT_SIGNING_KEY (env var). NUNCA en appsettings.json.
// Fallback a Jwt:Key solo para compatibilidad con User Secrets en desarrollo local.
var jwtKeyValue = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT signing key is not configured. " +
        "Set the JWT_SIGNING_KEY environment variable (minimum 64 bytes / 64 ASCII characters).");

var jwtKeyBytes = Encoding.UTF8.GetBytes(jwtKeyValue);
if (jwtKeyBytes.Length < 64)
    throw new InvalidOperationException(
        $"JWT_SIGNING_KEY must be at least 64 bytes long (got {jwtKeyBytes.Length}). " +
        "Generate a secure key with: openssl rand -base64 64");

const string JwtIssuer = "bulletin-dells";
const string JwtAudience = "bulletin-dells-client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = JwtIssuer,
            ValidAudience = JwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", limiter =>
    {
        limiter.PermitLimit = 100;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
        policy.WithOrigins(builder.Configuration["AllowedOrigins"]?.Split(',') ?? ["https://localhost:7031"])
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// Built-in OpenAPI (.NET 10)
builder.Services.AddOpenApi();

var app = builder.Build();

// Seed data in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

// Backfill published snapshots for any Approved listings that lack one (legacy / seeded data
// from before the snapshot column existed). No-op once everything has a snapshot — runs cheaply.
{
    using var scope = app.Services.CreateScope();
    var refresher = scope.ServiceProvider.GetRequiredService<IListingSnapshotRefresher>();
    var count = await refresher.BackfillMissingAsync();
    if (count > 0)
        Log.Information("Backfilled {Count} listing snapshots at startup.", count);
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
// Serves uploaded blobs from wwwroot/uploads/ — see LocalFilesystemBlobStorage.
// In production with cloud blob storage, this can be removed.
app.UseStaticFiles();
app.UseCors("BlazorClient");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow }))
    .WithTags("Health")
    .AllowAnonymous();

app.Run();

public partial class Program { }

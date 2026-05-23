using Bulletin.Board.Application.Settings;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using Bulletin.Board.Infrastructure.Identity;
using Bulletin.Board.Infrastructure.Persistence;
using Bulletin.Board.Infrastructure.Persistence.Repositories;
using Bulletin.Board.Infrastructure.Seed;
using Bulletin.Board.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bulletin.Board.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IListingRepository, ListingRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IAdminStatsRepository, AdminStatsRepository>();
        services.AddScoped<IAdminRatingsRepository, AdminRatingsRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IReservationRequestRepository, ReservationRequestRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Manage v3 read-side facades
        services.AddScoped<Application.Queries.Manage.IListingOverviewMetricsProvider, ListingOverviewMetricsProvider>();
        services.AddScoped<Application.Queries.Manage.IManageReviewsReader, ManageReviewsReader>();

        services.AddHttpContextAccessor();
        services.AddSingleton<Application.Storage.IBlobStorage, Storage.LocalFilesystemBlobStorage>();

        services.AddMemoryCache();
        services.AddSingleton<IReauthTokenService, MemoryReauthTokenService>();
        services.AddSingleton<IEncryptionService, EncryptionService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<DataSeeder>();

        services.Configure<NotificationSettings>(configuration.GetSection("Notifications"));

        // Background job: limpia refresh tokens expirados + revocados cada 24h
        services.AddHostedService<RefreshTokenCleanupService>();

        // Trending score: la SQL vive en TrendingScoreCalculator; el background service
        // (cada 1h) y el endpoint admin de recompute manual lo comparten.
        services.AddScoped<ITrendingScoreCalculator, TrendingScoreCalculator>();
        services.AddHostedService<TrendingScoreRecalculatorService>();

        return services;
    }
}

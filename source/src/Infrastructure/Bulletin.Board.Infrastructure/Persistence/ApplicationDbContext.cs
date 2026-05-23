using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bulletin.Board.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<ListingImage> ListingImages => Set<ListingImage>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<Verification> Verifications => Set<Verification>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<WaiverAcceptance> WaiverAcceptances => Set<WaiverAcceptance>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<VipChangeLog> VipChangeLogs => Set<VipChangeLog>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehiclePhoto> VehiclePhotos => Set<VehiclePhoto>();
    public DbSet<VehicleUnavailability> VehicleUnavailability => Set<VehicleUnavailability>();
    public DbSet<ReservationRequest> ReservationRequests => Set<ReservationRequest>();
    public DbSet<ReviewReply> ReviewReplies => Set<ReviewReply>();
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();
    public DbSet<ContactLog> ContactLogs => Set<ContactLog>();
    public DbSet<ListingAuditLog> ListingAuditLogs => Set<ListingAuditLog>();
    public DbSet<ListingView> ListingViews => Set<ListingView>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}

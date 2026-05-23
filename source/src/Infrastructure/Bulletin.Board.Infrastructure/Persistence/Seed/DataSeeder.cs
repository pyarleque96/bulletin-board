using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Infrastructure.Identity;
using Bulletin.Board.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Infrastructure.Seed;

// NOTE: seeded listings call Approve without a snapshot because images/vehicles are
// attached *after* approval in this file. Public reads fall back to live data when
// the snapshot column is null (see PR 3.2). Re-approving via the admin UI populates it.
public sealed class DataSeeder(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ILogger<DataSeeder> logger)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedRolesAsync(ct);
        await SeedUsersAsync(ct);
        await SeedCategoriesAsync(ct);
        await SeedListingsAsync(ct);
        await SeedManagePanelListingsAsync(ct);
    }

    private async Task SeedRolesAsync(CancellationToken ct)
    {
        string[] roles = ["Admin", "Provider", "User"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
                logger.LogInformation("Created role: {Role}", role);
            }
        }
    }

    private async Task SeedUsersAsync(CancellationToken ct)
    {
        await EnsureUserAsync("admin@bulletindells.com", "Admin123!", "Admin", "Carlos", "Mendoza", ct);
        await EnsureUserAsync("provider@bulletindells.com", "Provider123!", "Provider", "Rafael", "Guerrero", ct);
    }

    private async Task EnsureUserAsync(string email, string password, string role,
        string firstName, string lastName, CancellationToken ct)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            if (string.IsNullOrWhiteSpace(existing.FirstName) || string.IsNullOrWhiteSpace(existing.LastName))
            {
                existing.FirstName = firstName;
                existing.LastName = lastName;
                await userManager.UpdateAsync(existing);
            }
            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger.LogWarning("Failed to create seed user {Email}: {Errors}", email,
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(user, role);
        logger.LogInformation("Created seed user: {Email} with role {Role}", email, role);
    }

    private async Task SeedCategoriesAsync(CancellationToken ct)
    {
        var definitions = new[]
        {
            ("Car Rental",        "Renta de Autos",             "car-rental",    1, "🚗"),
            ("Housing",           "Vivienda",                   "housing",       2, "🏠"),
            ("Airport Rides",     "Transporte al Aeropuerto",   "airport-rides", 3, "✈️"),
            ("Mechanics",         "Mecánicos",                  "mechanics",     4, "🔧"),
            ("Beauty & Wellness", "Belleza y Bienestar",        "beauty-wellness", 5, "💆"),
            ("Jobs",              "Empleos",                    "jobs",          6, "💼"),
            ("Restaurants",       "Restaurantes",               "restaurants",   7, "🍽️"),
            ("Entertainment",     "Entretenimiento",            "entertainment", 8, "🎬"),
        };

        var existing = await context.Categories.ToListAsync(ct);
        var existingBySlugs = existing.ToDictionary(c => c.Slug);
        var added = 0;

        foreach (var (nameEn, nameEs, slug, order, icon) in definitions)
        {
            if (existingBySlugs.TryGetValue(slug, out var category))
            {
                category.Update(nameEn, nameEs, slug, order, icon);
            }
            else
            {
                await context.Categories.AddAsync(
                    Category.Create(nameEn, nameEs, slug, order, icon), ct);
                added++;
            }
        }

        await context.SaveChangesAsync(ct);
        logger.LogInformation("Categories seeded: {Added} added, {Updated} updated",
            added, definitions.Length - added);
    }

    private async Task SeedListingsAsync(CancellationToken ct)
    {
        if (await context.Listings.AnyAsync(ct)) return;

        var providerUser = await userManager.FindByEmailAsync("provider@bulletindells.com");
        if (providerUser is null)
        {
            logger.LogWarning("Provider user not found, skipping listing seed");
            return;
        }

        var categories = await context.Categories.ToListAsync(ct);
        var carRentalCat = categories.First(c => c.Slug == "car-rental");
        var housingCat = categories.First(c => c.Slug == "housing");
        var airportCat = categories.First(c => c.Slug == "airport-rides");
        var mechanicsCat = categories.First(c => c.Slug == "mechanics");
        var beautyCat = categories.First(c => c.Slug == "beauty-wellness");

        // Provider VIP principal (el provider registrado en el seed)
        var vipFeaturedProvider = await EnsureProviderAsync(
            providerUser.Id, "+15715550194",
            VerificationStatus.Approved, ProviderTier.VIP, ct);

        var vipProvider1 = await EnsureProviderAsync(
            Guid.NewGuid(), "+15715550195",
            VerificationStatus.Approved, ProviderTier.VIP, ct,
            createUser: true, email: "vip1@bulletindells.com",
            firstName: "Marco", lastName: "Delgado");

        var vipProvider2 = await EnsureProviderAsync(
            Guid.NewGuid(), "+15715550196",
            VerificationStatus.Approved, ProviderTier.VIP, ct,
            createUser: true, email: "vip2@bulletindells.com",
            firstName: "Sandra", lastName: "Reyes");

        var verProvider1 = await EnsureProviderAsync(
            Guid.NewGuid(), "+15715550197",
            VerificationStatus.Approved, ProviderTier.Verified, ct,
            createUser: true, email: "ver1@bulletindells.com",
            firstName: "Jorge", lastName: "Castillo");

        var verProvider2 = await EnsureProviderAsync(
            Guid.NewGuid(), "+15715550198",
            VerificationStatus.Approved, ProviderTier.Verified, ct,
            createUser: true, email: "ver2@bulletindells.com",
            firstName: "Marisol", lastName: "Fuentes");

        var verProvider3 = await EnsureProviderAsync(
            Guid.NewGuid(), "+15715550199",
            VerificationStatus.Approved, ProviderTier.Verified, ct,
            createUser: true, email: "ver3@bulletindells.com",
            firstName: "Alejandro", lastName: "Vega");

        var verProvider4 = await EnsureProviderAsync(
            Guid.NewGuid(), "+15715550200",
            VerificationStatus.Approved, ProviderTier.Verified, ct,
            createUser: true, email: "ver4@bulletindells.com",
            firstName: "Lucía", lastName: "Torres");

        await context.SaveChangesAsync(ct);

        // Listing 1: VIP — Car Rental Hero
        var featuredListing = Listing.Create(
            providerId: vipFeaturedProvider.Id,
            categoryId: carRentalCat.Id,
            providerTier: ProviderTier.VIP,
            titleEn: "Dells Premier Car Rentals",
            titleEs: "Renta de Autos Premium Dells",
            descriptionEn: "Premium car rental service in Wisconsin Dells. Best vehicles at competitive prices. Daily, weekly and monthly rates available.",
            descriptionEs: "Servicio premium de renta de autos en Wisconsin Dells. Los mejores vehículos a precios competitivos. Tarifas diarias, semanales y mensuales disponibles.",
            price: 39m,
            location: "Wisconsin Dells, WI",
            whatsAppNumber: "+15715550194");

        featuredListing.Approve(Guid.NewGuid());
        featuredListing.UpdateAvgRating(4.9m);
        await context.Listings.AddAsync(featuredListing, ct);
        await context.SaveChangesAsync(ct);

        AddPublicImages(featuredListing.Id, [
            ("https://picsum.photos/seed/car-rental-1/800/600", 1),
            ("https://picsum.photos/seed/car-rental-2/800/600", 2),
            ("https://picsum.photos/seed/car-rental-3/800/600", 3)
        ]);

        // Listing 2: VIP - Airport Rides
        var vipAirportListing = Listing.Create(
            providerId: vipProvider1.Id,
            categoryId: airportCat.Id,
            providerTier: ProviderTier.VIP,
            titleEn: "Dells Airport Express",
            titleEs: "Express Aeropuerto Dells",
            descriptionEn: "Premium airport transportation. MSN and MKE airports. Comfortable and punctual service.",
            descriptionEs: "Transporte premium al aeropuerto. Aeropuertos MSN y MKE. Servicio cómodo y puntual.",
            price: 85m,
            location: "Wisconsin Dells, WI",
            whatsAppNumber: "+15715550195");

        vipAirportListing.Approve(Guid.NewGuid());
        vipAirportListing.UpdateAvgRating(4.7m);
        await context.Listings.AddAsync(vipAirportListing, ct);
        await context.SaveChangesAsync(ct);

        AddPublicImages(vipAirportListing.Id, [
            ("https://picsum.photos/seed/airport-rides-1/800/600", 1),
            ("https://picsum.photos/seed/airport-rides-2/800/600", 2)
        ]);

        // Listing 3: VIP - Housing
        var vipHousingListing = Listing.Create(
            providerId: vipProvider2.Id,
            categoryId: housingCat.Id,
            providerTier: ProviderTier.VIP,
            titleEn: "Casa Dells Housing",
            titleEs: "Casa Dells Vivienda",
            descriptionEn: "Premium housing options for workers and visitors. Fully furnished rooms and apartments.",
            descriptionEs: "Opciones de vivienda premium para trabajadores y visitantes. Habitaciones y apartamentos completamente amueblados.",
            price: 150m,
            location: "Wisconsin Dells, WI",
            whatsAppNumber: "+15715550196");

        vipHousingListing.Approve(Guid.NewGuid());
        vipHousingListing.UpdateAvgRating(4.6m);
        await context.Listings.AddAsync(vipHousingListing, ct);
        await context.SaveChangesAsync(ct);

        AddPublicImages(vipHousingListing.Id, [
            ("https://picsum.photos/seed/housing-1/800/600", 1),
            ("https://picsum.photos/seed/housing-2/800/600", 2)
        ]);

        // Listing 4: Verified - Mechanics
        var mechanicsListing = Listing.Create(
            providerId: verProvider1.Id,
            categoryId: mechanicsCat.Id,
            providerTier: ProviderTier.Verified,
            titleEn: "Dells Auto Repair",
            titleEs: "Reparación de Autos Dells",
            descriptionEn: "Trusted auto repair shop. Oil changes, brakes, tires, engine diagnostics.",
            descriptionEs: "Taller de reparación de confianza. Cambios de aceite, frenos, llantas, diagnóstico del motor.",
            price: 60m,
            location: "Wisconsin Dells, WI",
            whatsAppNumber: "+15715550197");

        mechanicsListing.Approve(Guid.NewGuid());
        mechanicsListing.UpdateAvgRating(4.3m);
        await context.Listings.AddAsync(mechanicsListing, ct);
        await context.SaveChangesAsync(ct);

        AddPublicImages(mechanicsListing.Id, [
            ("https://picsum.photos/seed/mechanics-1/800/600", 1)
        ]);

        // Listing 5: Verified - Beauty
        var beautyListing = Listing.Create(
            providerId: verProvider2.Id,
            categoryId: beautyCat.Id,
            providerTier: ProviderTier.Verified,
            titleEn: "Marisol's Beauty Studio",
            titleEs: "Estudio de Belleza Marisol",
            descriptionEn: "Full beauty services. Hair, nails, makeup and more. Walk-ins welcome.",
            descriptionEs: "Servicios completos de belleza. Cabello, uñas, maquillaje y más. Sin cita bienvenido.",
            price: 45m,
            location: "Wisconsin Dells, WI",
            whatsAppNumber: "+15715550198");

        beautyListing.Approve(Guid.NewGuid());
        beautyListing.UpdateAvgRating(4.5m);
        await context.Listings.AddAsync(beautyListing, ct);
        await context.SaveChangesAsync(ct);

        AddPublicImages(beautyListing.Id, [
            ("https://picsum.photos/seed/beauty-wellness-1/800/600", 1)
        ]);

        // Listing 6: Verified - Housing #2
        var housingListing2 = Listing.Create(
            providerId: verProvider3.Id,
            categoryId: housingCat.Id,
            providerTier: ProviderTier.Verified,
            titleEn: "Dells Comfortable Rooms",
            titleEs: "Habitaciones Confortables Dells",
            descriptionEn: "Clean and affordable rooms for workers and seasonal employees.",
            descriptionEs: "Habitaciones limpias y accesibles para trabajadores y empleados de temporada.",
            price: 100m,
            location: "Wisconsin Dells, WI",
            whatsAppNumber: "+15715550199");

        housingListing2.Approve(Guid.NewGuid());
        housingListing2.UpdateAvgRating(4.1m);
        await context.Listings.AddAsync(housingListing2, ct);
        await context.SaveChangesAsync(ct);

        AddPublicImages(housingListing2.Id, [
            ("https://picsum.photos/seed/housing-alt-1/800/600", 1)
        ]);

        // Listing 7: Verified - Airport #2
        var airportListing2 = Listing.Create(
            providerId: verProvider4.Id,
            categoryId: airportCat.Id,
            providerTier: ProviderTier.Verified,
            titleEn: "Dells Rides & Transport",
            titleEs: "Dells Viajes y Transporte",
            descriptionEn: "Reliable rides to MSN airport and surrounding areas. 24/7 availability.",
            descriptionEs: "Viajes confiables al aeropuerto MSN y áreas circundantes. Disponibilidad 24/7.",
            price: 70m,
            location: "Wisconsin Dells, WI",
            whatsAppNumber: "+15715550200");

        airportListing2.Approve(Guid.NewGuid());
        airportListing2.UpdateAvgRating(4.4m);
        await context.Listings.AddAsync(airportListing2, ct);

        await context.SaveChangesAsync(ct);
        logger.LogInformation("Seeded listings successfully");

        void AddPublicImages(Guid listingId, (string url, int order)[] images)
        {
            foreach (var (url, order) in images)
            {
                var img = ListingImage.Create(listingId, url, order);
                img.MakePublic();
                context.ListingImages.Add(img);
            }
        }
    }

    private async Task SeedManagePanelListingsAsync(CancellationToken ct)
    {
        var providerUser = await userManager.FindByEmailAsync("provider@bulletindells.com");
        if (providerUser is null) return;

        var provider = await context.Providers.FirstOrDefaultAsync(p => p.UserId == providerUser.Id, ct);
        if (provider is null) return;

        var categories = await context.Categories.ToListAsync(ct);
        var housingCat   = categories.First(c => c.Slug == "housing");
        var carRentalCat = categories.First(c => c.Slug == "car-rental");
        var airportCat   = categories.First(c => c.Slug == "airport-rides");
        var mechanicsCat = categories.First(c => c.Slug == "mechanics");
        var beautyCat    = categories.First(c => c.Slug == "beauty-wellness");
        var jobsCat      = categories.First(c => c.Slug == "jobs");

        var demoListings = new (string TitleEn, string TitleEs, Guid CategoryId, string State, decimal Price, string Wa, decimal Rating)[]
        {
            ("Studio Apt near Duck Pond",     "Estudio cerca de Duck Pond",     housingCat.Id,   "Pending",      950m, "+15715550194", 0m),
            ("Maria's Mobile Hair Salon",     "Salón Móvil de Maria",           beautyCat.Id,    "Approved",     45m,  "+15715550194", 4.8m),
            ("Seasonal Lifeguard Position",   "Puesto de Salvavidas Temporal",  jobsCat.Id,      "NeedsChanges", 18m,  "+15715550194", 0m),
            ("Quick Lube & Brake Service",    "Cambio de Aceite y Frenos",      mechanicsCat.Id, "Approved",     59m,  "+15715550194", 4.4m),
            ("Airport Shuttle - MSN & MKE",   "Shuttle Aeropuerto MSN y MKE",   airportCat.Id,   "Rejected",     75m,  "+15715550194", 0m),
            ("Compact SUV Daily Rental",      "Renta Diaria SUV Compacta",      carRentalCat.Id, "Approved",     49m,  "+15715550194", 4.6m),
            ("Downtown Loft - Weekly",        "Loft Centro - Semanal",          housingCat.Id,   "Approved",     420m, "+15715550194", 4.2m),
            ("Resort Housekeeping Help",      "Ayuda en Limpieza de Resort",    jobsCat.Id,      "Pending",      16m,  "+15715550194", 0m),
        };

        var existing = await context.Listings
            .Where(l => l.ProviderId == provider.Id)
            .Select(l => l.TitleEn)
            .ToListAsync(ct);
        var existingTitles = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);

        var added = 0;
        foreach (var (titleEn, titleEs, categoryId, state, price, wa, rating) in demoListings)
        {
            if (existingTitles.Contains(titleEn)) continue;

            var listing = Listing.Create(
                providerId: provider.Id,
                categoryId: categoryId,
                providerTier: ProviderTier.Verified,
                titleEn: titleEn,
                titleEs: titleEs,
                descriptionEn: $"{titleEn} — demo seed listing for the Manage panel.",
                descriptionEs: $"{titleEs} — anuncio demo para el panel de gestión.",
                price: price,
                location: "Wisconsin Dells, WI",
                whatsAppNumber: wa);

            switch (state)
            {
                case "Approved":
                    listing.Approve(Guid.NewGuid());
                    if (rating > 0) listing.UpdateAvgRating(rating);
                    break;
                case "Rejected":
                    listing.Reject("Photos do not meet the platform's quality standards. Please upload clearer images.");
                    break;
                case "NeedsChanges":
                    listing.RequestChanges("Please provide a more detailed description and update the pricing range.");
                    break;
                case "Pending":
                default:
                    break;
            }

            await context.Listings.AddAsync(listing, ct);
            added++;
        }

        if (added > 0)
        {
            await context.SaveChangesAsync(ct);
            logger.LogInformation("Manage panel demo seeded: {Added} listings for provider@bulletindells.com", added);
        }
    }

    private async Task<Provider> EnsureProviderAsync(
        Guid userId,
        string whatsApp,
        VerificationStatus verificationStatus,
        ProviderTier tier,
        CancellationToken ct,
        bool createUser = false,
        string? email = null,
        string? firstName = null,
        string? lastName = null)
    {
        var existing = await context.Providers.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (existing is not null)
        {
            // Backfill nombre del ApplicationUser si quedó vacío en seeds previas.
            if (createUser && email is not null && (firstName is not null || lastName is not null))
            {
                var u = await userManager.FindByEmailAsync(email);
                if (u is not null && (string.IsNullOrWhiteSpace(u.FirstName) || string.IsNullOrWhiteSpace(u.LastName)))
                {
                    u.FirstName = firstName;
                    u.LastName = lastName;
                    await userManager.UpdateAsync(u);
                }
            }
            return existing;
        }

        if (createUser && email is not null)
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser is null)
            {
                var newUser = new ApplicationUser
                {
                    Id = userId,
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName
                };
                await userManager.CreateAsync(newUser, "Provider123!");
                await userManager.AddToRoleAsync(newUser, "Provider");
            }
            else
            {
                userId = existingUser.Id;
                if (string.IsNullOrWhiteSpace(existingUser.FirstName) && firstName is not null)
                {
                    existingUser.FirstName = firstName;
                    existingUser.LastName = lastName;
                    await userManager.UpdateAsync(existingUser);
                }
            }
        }

        var provider = Provider.Create(userId, whatsApp);
        if (verificationStatus == VerificationStatus.Approved)
            provider.ApproveVerification();

        provider.ApproveTier(tier);

        await context.Providers.AddAsync(provider, ct);
        return provider;
    }
}

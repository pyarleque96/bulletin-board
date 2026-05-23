using Bulletin.Board.Application.Queries.Listings;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Bulletin.Board.Application.Tests.Queries.Listings;

public class GetTopListingsQueryHandlerTests
{
    private readonly IListingRepository _repo = Substitute.For<IListingRepository>();
    private readonly GetTopListingsQueryHandler _sut;

    public GetTopListingsQueryHandlerTests()
    {
        _sut = new GetTopListingsQueryHandler(_repo);
    }

    [Fact]
    public async Task Diversity_cap_keeps_at_most_two_listings_per_provider()
    {
        // Same provider with 5 listings + a different provider with 2 — the cap should
        // keep only the first 2 from provider A and both from provider B, even though
        // the repository returned them all in trending order.
        var providerA = Guid.NewGuid();
        var providerB = Guid.NewGuid();
        var trending = new List<Listing>
        {
            CreateApproved("A1", providerA),
            CreateApproved("A2", providerA),
            CreateApproved("A3", providerA),  // skipped by cap
            CreateApproved("A4", providerA),  // skipped by cap
            CreateApproved("B1", providerB),
            CreateApproved("A5", providerA),  // skipped by cap
            CreateApproved("B2", providerB),
        };

        _repo.GetTrendingAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(trending);

        var result = await _sut.Handle(new GetTopListingsQuery(Count: 10), default);

        result.Should().HaveCount(4);
        result.Count(c => c.TitleEn == "A1").Should().Be(1);
        result.Count(c => c.TitleEn == "A2").Should().Be(1);
        result.Count(c => c.TitleEn == "B1").Should().Be(1);
        result.Count(c => c.TitleEn == "B2").Should().Be(1);
        result.Any(c => c.TitleEn == "A3" || c.TitleEn == "A4" || c.TitleEn == "A5").Should().BeFalse();
    }

    [Fact]
    public async Task Preserves_repository_ordering_when_cap_does_not_kick_in()
    {
        var trending = new List<Listing>
        {
            CreateApproved("first",  Guid.NewGuid()),
            CreateApproved("second", Guid.NewGuid()),
            CreateApproved("third",  Guid.NewGuid()),
        };

        _repo.GetTrendingAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(trending);

        var result = await _sut.Handle(new GetTopListingsQuery(Count: 10), default);

        result.Select(c => c.TitleEn).Should().Equal("first", "second", "third");
    }

    [Theory]
    [InlineData(0,   10)]   // default when 0
    [InlineData(-3,  10)]   // default when negative
    [InlineData(5,    5)]
    [InlineData(60,  50)]   // capped at 50
    public async Task Count_is_clamped_and_drives_request_size(int requested, int expectedMax)
    {
        _repo.GetTrendingAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(new List<Listing>());

        await _sut.Handle(new GetTopListingsQuery(Count: requested), default);

        // Over-fetch is size * 3 capped at 60. Verify the repository call is consistent with
        // that envelope — concrete take depends on min(expectedMax*3, 60).
        var expectedFetch = Math.Min(expectedMax * 3, 60);
        await _repo.Received(1).GetTrendingAsync(expectedFetch, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Stops_at_requested_count_even_when_repo_returns_more()
    {
        var trending = Enumerable.Range(0, 15)
            .Select(i => CreateApproved($"L{i}", Guid.NewGuid()))
            .ToList();

        _repo.GetTrendingAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(trending);

        var result = await _sut.Handle(new GetTopListingsQuery(Count: 5), default);

        result.Should().HaveCount(5);
    }

    private static Listing CreateApproved(string titleEn, Guid providerId)
    {
        var listing = Listing.Create(
            providerId: providerId,
            categoryId: Guid.NewGuid(),
            providerTier: ProviderTier.VIP,
            titleEn: titleEn,
            titleEs: titleEn);
        listing.Approve(Guid.NewGuid());
        return listing;
    }
}

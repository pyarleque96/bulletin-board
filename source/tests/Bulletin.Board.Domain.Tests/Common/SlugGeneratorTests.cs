using Bulletin.Board.Domain.Common;
using FluentAssertions;

namespace Bulletin.Board.Domain.Tests.Common;

public class SlugGeneratorTests
{
    [Theory]
    [InlineData("Dells Premier Car Rentals", "dells-premier-car-rentals")]
    [InlineData("  trim me  ", "trim-me")]
    [InlineData("UPPER case", "upper-case")]
    public void Slugify_normalizes_whitespace_and_case(string input, string expected)
    {
        SlugGenerator.Slugify(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("Niño Pequeño", "nino-pequeno")]
    [InlineData("Café André", "cafe-andre")]
    [InlineData("São Paulo", "sao-paulo")]
    [InlineData("Über cool", "uber-cool")]
    public void Slugify_strips_diacritics(string input, string expected)
    {
        SlugGenerator.Slugify(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("hello!@#world", "hello-world")]
    [InlineData("a---b", "a-b")]
    [InlineData("a___b", "a-b")]
    [InlineData("---leading-and-trailing---", "leading-and-trailing")]
    [InlineData("with.dots.and/slashes", "with-dots-and-slashes")]
    public void Slugify_collapses_special_chars_and_runs_of_dashes(string input, string expected)
    {
        SlugGenerator.Slugify(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!@#$%")]
    [InlineData("---")]
    public void Slugify_falls_back_when_input_yields_empty(string? input)
    {
        SlugGenerator.Slugify(input).Should().Be("listing");
    }

    [Fact]
    public void Slugify_truncates_to_max_length_and_trims_trailing_dash()
    {
        // Construct a string that, after slugification, would land a '-' at position maxLength.
        // "aa-bb-cc-..." pattern with explicit slug length 10 should not end with '-'.
        var input = "aa bb cc dd ee ff gg hh";
        var result = SlugGenerator.Slugify(input, maxLength: 10);

        result.Length.Should().BeLessThanOrEqualTo(10);
        result.Should().NotEndWith("-");
        result.Should().StartWith("aa-bb-cc");
    }

    [Fact]
    public void Slugify_respects_default_max_length_of_120()
    {
        var input = new string('a', 200);
        var result = SlugGenerator.Slugify(input);

        result.Should().HaveLength(120);
        result.Should().Be(new string('a', 120));
    }

    [Fact]
    public void Slugify_throws_when_max_length_not_positive()
    {
        var act = () => SlugGenerator.Slugify("hello", maxLength: 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData("car-rental", 2, "car-rental-2")]
    [InlineData("car-rental", 10, "car-rental-10")]
    public void AppendSuffix_appends_collision_suffix(string baseSlug, int suffix, string expected)
    {
        SlugGenerator.AppendSuffix(baseSlug, suffix).Should().Be(expected);
    }

    [Fact]
    public void AppendSuffix_truncates_base_when_suffix_would_overflow()
    {
        var baseSlug = new string('a', 120);
        var result = SlugGenerator.AppendSuffix(baseSlug, 99);

        result.Length.Should().BeLessThanOrEqualTo(120);
        result.Should().EndWith("-99");
        result.Should().NotContain("--");
    }

    [Fact]
    public void AppendSuffix_throws_on_zero_or_negative_suffix()
    {
        var act = () => SlugGenerator.AppendSuffix("car-rental", 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AppendSuffix_throws_on_empty_base(string? baseSlug)
    {
        var act = () => SlugGenerator.AppendSuffix(baseSlug!, 2);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Slugify_output_matches_db_check_constraint_pattern()
    {
        // The DB CHECK constraint is: slug ~ '^[a-z0-9]+(-[a-z0-9]+)*$'
        // Every Slugify output must satisfy it (or be the fallback "listing").
        var samples = new[]
        {
            "Dells Premier Car Rentals",
            "Niño Pequeño",
            "hello!@#world",
            "a---b",
            "  trim me  ",
            "São Paulo"
        };

        var pattern = new System.Text.RegularExpressions.Regex("^[a-z0-9]+(-[a-z0-9]+)*$");

        foreach (var sample in samples)
        {
            var slug = SlugGenerator.Slugify(sample);
            pattern.IsMatch(slug).Should().BeTrue($"slug '{slug}' (from '{sample}') must satisfy the DB check constraint");
        }
    }
}

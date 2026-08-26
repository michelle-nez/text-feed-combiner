using FeedCombiner.Core;

namespace FeedCombiner.Tests;

/// <summary>
/// The marketplace if/else chain and the Split('-') naming rule, now testable on
/// their own instead of being reachable only by dropping files on a form.
/// </summary>
public class NamingTests
{
    private static readonly MarketplaceDetector Detector = new();
    private static readonly OutputNameBuilder Builder = new(Detector);

    [Theory]
    [InlineData("RITEAV-Q4-AMAZON-feed.txt", "Amazon")]
    [InlineData("store-SHOPIFY-export.txt", "Shopify")]
    [InlineData("listings-EBAY-2026.txt", "eBay")]
    [InlineData("bulk-WALMART-items.txt", "Walmart")]
    public void Detects_the_marketplace_from_the_file_name(string fileName, string expected)
    {
        Assert.Equal(expected, Detector.Detect(fileName)?.Marketplace);
    }

    [Fact]
    public void Marketplace_match_is_case_insensitive()
    {
        Assert.Equal("Amazon", Detector.Detect("riteav-amazon-feed.txt")?.Marketplace);
    }

    [Fact]
    public void Unknown_marketplace_falls_back_to_the_default_binder()
    {
        Assert.Null(Detector.Detect("random-export.txt"));
        Assert.Equal(MarketplaceDetector.DefaultBinderName, Detector.BinderNameFor("random-export.txt"));
    }

    [Fact]
    public void Binder_is_a_text_file_not_an_xls()
    {
        // The original named a tab-delimited file ".xls", so Excel warned on open.
        Assert.EndsWith(".txt", Detector.BinderNameFor("RITEAV-Q4-AMAZON-feed.txt"));
    }

    [Fact]
    public void Output_name_keeps_the_first_two_segments_of_the_source_name()
    {
        var name = Builder.Build(new FeedFile("RITEAV-Q4-AMAZON-feed.txt", Array.Empty<string>()));
        Assert.Equal("RITEAV-Q4-Binder_AMAZON.txt", name);
    }

    [Fact]
    public void Output_name_uses_the_whole_base_name_when_there_is_no_hyphen()
    {
        var name = Builder.Build(new FeedFile("export.txt", Array.Empty<string>()));
        Assert.Equal("export-Binder.txt", name);
    }

    [Fact]
    public void Feed_file_requires_a_name()
    {
        Assert.Throws<ArgumentException>(() => new FeedFile(" ", Array.Empty<string>()));
    }

    [Fact]
    public void Base_name_strips_only_the_final_extension()
    {
        Assert.Equal("feed.2026", new FeedFile("feed.2026.txt", Array.Empty<string>()).BaseName);
    }
}

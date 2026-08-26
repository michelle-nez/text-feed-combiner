namespace FeedCombiner.Core;

/// <summary>
/// Runs the marketplace rules in order and falls back to a generic binder name.
/// </summary>
public class MarketplaceDetector
{
    public const string DefaultBinderName = "Binder.txt";

    private readonly IReadOnlyList<IMarketplaceRule> rules;

    public MarketplaceDetector(IReadOnlyList<IMarketplaceRule>? rules = null)
    {
        this.rules = rules ?? Default();
    }

    /// <summary>The marketplaces the original app knew about.</summary>
    public static IReadOnlyList<IMarketplaceRule> Default() => new List<IMarketplaceRule>
    {
        new KeywordMarketplaceRule("Amazon", "AMAZON"),
        new KeywordMarketplaceRule("Shopify", "SHOPIFY"),
        new KeywordMarketplaceRule("eBay", "EBAY"),
        new KeywordMarketplaceRule("Walmart", "WALMART")
    };

    public IMarketplaceRule? Detect(string fileName) =>
        rules.FirstOrDefault(rule => rule.Matches(fileName));

    public string BinderNameFor(string fileName) =>
        Detect(fileName)?.BinderName ?? DefaultBinderName;
}

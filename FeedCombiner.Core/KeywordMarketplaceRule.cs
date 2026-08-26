namespace FeedCombiner.Core;

/// <summary>
/// Recognises a marketplace when its name appears anywhere in the file name.
/// </summary>
public class KeywordMarketplaceRule : IMarketplaceRule
{
    private readonly string keyword;

    public KeywordMarketplaceRule(string marketplace, string keyword)
    {
        Marketplace = marketplace;
        this.keyword = keyword;
    }

    public string Marketplace { get; }

    // The original hard-coded ".xls" even though the content is tab-delimited
    // text, so Excel warned on every open. Text files get a .txt extension.
    public string BinderName => $"Binder_{Marketplace.ToUpperInvariant()}.txt";

    public bool Matches(string fileName) =>
        fileName.Contains(keyword, StringComparison.OrdinalIgnoreCase);
}

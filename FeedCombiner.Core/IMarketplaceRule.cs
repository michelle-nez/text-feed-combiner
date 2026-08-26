namespace FeedCombiner.Core;

/// <summary>
/// One rule that recognises a marketplace from a file name. The original app had
/// this as an if/else chain inside a drag-drop handler; as a rule, each case can
/// be tested on its own and new marketplaces are added without touching a branch.
/// </summary>
public interface IMarketplaceRule
{
    string Marketplace { get; }

    bool Matches(string fileName);

    string BinderName { get; }
}

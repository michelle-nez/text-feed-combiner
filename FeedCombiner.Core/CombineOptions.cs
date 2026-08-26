namespace FeedCombiner.Core;

/// <summary>
/// The choices the original app made silently. Making them explicit is half the
/// point of the rewrite.
/// </summary>
public class CombineOptions
{
    /// <summary>Drop duplicate lines. The original always did this.</summary>
    public bool RemoveDuplicates { get; set; } = true;

    /// <summary>Treat the first line of each file as a header and keep only the first one.</summary>
    public bool FirstLineIsHeader { get; set; }

    /// <summary>Drop lines that are empty or whitespace.</summary>
    public bool SkipBlankLines { get; set; } = true;

    /// <summary>Trim trailing whitespace before comparing lines for duplicates.</summary>
    public bool TrimLineEnds { get; set; } = true;
}

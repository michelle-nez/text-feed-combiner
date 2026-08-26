namespace FeedCombiner.Core;

/// <summary>
/// What happened, as data. The original reported success with a commented-out
/// message box and failure with a different one, so nothing could be shown on
/// screen or asserted in a test.
/// </summary>
public class CombineResult
{
    public CombineResult(string outputName, IReadOnlyList<string> lines,
                         int filesProcessed, int linesRead,
                         int duplicatesRemoved, int blanksSkipped,
                         string? marketplace)
    {
        OutputName = outputName;
        Lines = lines;
        FilesProcessed = filesProcessed;
        LinesRead = linesRead;
        DuplicatesRemoved = duplicatesRemoved;
        BlanksSkipped = blanksSkipped;
        Marketplace = marketplace;
    }

    public string OutputName { get; }

    public IReadOnlyList<string> Lines { get; }

    public int FilesProcessed { get; }

    public int LinesRead { get; }

    public int DuplicatesRemoved { get; }

    public int BlanksSkipped { get; }

    /// <summary>Null when no marketplace rule matched.</summary>
    public string? Marketplace { get; }

    public int LinesWritten => Lines.Count;

    public string ToText() => string.Join(Environment.NewLine, Lines);
}

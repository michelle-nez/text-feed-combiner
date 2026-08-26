namespace FeedCombiner.Core;

/// <summary>
/// Combines feed files into one, dropping duplicates. This is the logic that used
/// to live inside Form1_DragDrop, with no UI and no file system in sight.
/// </summary>
public class FeedCombinerService
{
    private readonly MarketplaceDetector detector;
    private readonly OutputNameBuilder nameBuilder;

    public FeedCombinerService(MarketplaceDetector detector, OutputNameBuilder nameBuilder)
    {
        this.detector = detector;
        this.nameBuilder = nameBuilder;
    }

    public CombineResult Combine(IReadOnlyList<FeedFile> files, CombineOptions? options = null)
    {
        if (files is null || files.Count == 0)
        {
            throw new ArgumentException("Give me at least one file to combine.", nameof(files));
        }

        options ??= new CombineOptions();

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var output = new List<string>();

        var linesRead = 0;
        var duplicates = 0;
        var blanks = 0;
        var headerTaken = false;

        foreach (var file in files)
        {
            var isFirstLineOfFile = true;

            foreach (var raw in file.Lines)
            {
                linesRead++;

                var line = options.TrimLineEnds ? raw.TrimEnd() : raw;

                // A header should appear once, from the first file only.
                if (options.FirstLineIsHeader && isFirstLineOfFile)
                {
                    isFirstLineOfFile = false;

                    if (headerTaken)
                    {
                        duplicates++;
                        continue;
                    }

                    headerTaken = true;
                    seen.Add(line);
                    output.Add(line);
                    continue;
                }

                isFirstLineOfFile = false;

                if (options.SkipBlankLines && string.IsNullOrWhiteSpace(line))
                {
                    blanks++;
                    continue;
                }

                if (options.RemoveDuplicates && !seen.Add(line))
                {
                    duplicates++;
                    continue;
                }

                output.Add(line);
            }
        }

        var first = files[0];

        return new CombineResult(
            outputName: nameBuilder.Build(first),
            lines: output,
            filesProcessed: files.Count,
            linesRead: linesRead,
            duplicatesRemoved: duplicates,
            blanksSkipped: blanks,
            marketplace: detector.Detect(first.Name)?.Marketplace);
    }
}

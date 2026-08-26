namespace FeedCombiner.Core;

/// <summary>
/// Builds the output file name from the first file's name plus the binder name.
/// Lifted verbatim in behaviour from the original's Split('-') logic, but as its
/// own class so it can be tested without dropping files on a form.
/// </summary>
public class OutputNameBuilder
{
    private readonly MarketplaceDetector detector;

    public OutputNameBuilder(MarketplaceDetector detector)
    {
        this.detector = detector;
    }

    public string Build(FeedFile first)
    {
        var binder = detector.BinderNameFor(first.Name);
        var parts = first.BaseName.Split('-');

        // Two or more segments: keep the first two as the prefix, matching the
        // original. Otherwise use the whole base name.
        return parts.Length >= 2
            ? $"{parts[0]}-{parts[1]}-{binder}"
            : $"{first.BaseName}-{binder}";
    }
}

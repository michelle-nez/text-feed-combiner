namespace FeedCombiner.Core;

/// <summary>
/// One uploaded feed file. Holds a name and its lines - nothing about where it
/// came from, so the domain never touches the file system or the browser.
/// </summary>
public class FeedFile
{
    public FeedFile(string name, IReadOnlyList<string> lines)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A feed file needs a name.", nameof(name));
        }

        Name = name;
        Lines = lines ?? throw new ArgumentNullException(nameof(lines));
    }

    public string Name { get; }

    public IReadOnlyList<string> Lines { get; }

    public int LineCount => Lines.Count;

    /// <summary>The name without its extension, used to build the output name.</summary>
    public string BaseName
    {
        get
        {
            var dot = Name.LastIndexOf('.');
            return dot > 0 ? Name[..dot] : Name;
        }
    }
}

using Microsoft.Extensions.Caching.Memory;

namespace FeedCombiner.Web;

/// <summary>
/// Holds a finished combine result just long enough for the browser to fetch it.
///
/// The download is a normal HTTP GET, and a GET is a separate request from the
/// Blazor circuit that produced the result - so the result cannot simply be read
/// from the component. Parking it here under a short id bridges the two, and the
/// entry expires on its own so nothing accumulates.
/// </summary>
public class CombinedFileStore
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);

    private readonly IMemoryCache cache;

    public CombinedFileStore(IMemoryCache cache)
    {
        this.cache = cache;
    }

    public string Put(string fileName, string content)
    {
        var id = Guid.NewGuid().ToString("N");
        cache.Set(Key(id), new StoredFile(fileName, content), Lifetime);
        return id;
    }

    public StoredFile? Get(string id) =>
        cache.TryGetValue(Key(id), out StoredFile? file) ? file : null;

    private static string Key(string id) => $"combined:{id}";

    public record StoredFile(string FileName, string Content);
}

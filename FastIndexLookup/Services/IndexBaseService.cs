using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace IndexService;

public class IndexBaseService : IIndexBaseService
{
    /// Instead of using a dictionary we could use a ConcurrentDictionary?
    /// 
    /// Initially I can assume that I'd like to maintain the index in memory but want to keep the memory footprint as small as i can
    /// as i need a lot of memory for my other processes.
    /// 
    /// So we use a MemoryCache with expiry instead?
    /// This interferes with Orleans grain manangment and memory caching options 🤯
    /// 
    /// IndexEntry needs to be a custom cache object
    private readonly ConcurrentDictionary<string, Cache> IndexEntries = new();

    public ConcurrentDictionary<string, Cache> GetIndexEntries() => IndexEntries;

    private readonly TimeSpan SlidingExpiration;

    public IndexBaseService(IConfigurationBootStrapper bootStrapper)
    {
        var config = bootStrapper.GetConfig();
        var cacheExpiry = config.GetSection("CacheSettings").Get<CacheSettings>();
        if (cacheExpiry != null)
        {
            SlidingExpiration = cacheExpiry.SlidingExpiration;
        }
        else
        {
            throw new NullReferenceException("SlidingExpiration cannot be null in appSettings.json");
        }
        _ = new Timer(ExpireIndexEntries, null, SlidingExpiration, SlidingExpiration);
    }

    public void LoadIndexEntries(IEnumerable<IndexEntry> entries)
    {
        foreach (var entry in entries)
        {
            Upsert(entry);
        }
    }

    public void Upsert(IndexEntry entry)
    {
        ValidateType(entry.Type);
        var key = CreateKey(entry);
        if (IndexEntries.TryGetValue(key, out var existing))
        {
            var mergedIds = new HashSet<Guid>(existing.Entry.IDs);
            foreach (var id in entry.IDs) mergedIds.Add(id);
            IndexEntries[key] = new Cache(existing.Entry with { IDs = mergedIds.ToArray()}, DateTime.Now);
        }
        else
        {
            IndexEntries[key] = new Cache(entry, DateTime.Now);
        }
    }

    public bool TryGetValue(string key, [NotNullWhen(true)] out IndexEntry? result)
    {
        if (IndexEntries.TryGetValue(key, out var cache))
        {
            cache.Time = DateTime.Now;
            result = cache.Entry;
            return true;
        }
        
        // Dispose
        result = null;
        
        return false;
    }

    private static void ValidateType(IdentifierType type)
    {
        if (!Enum.IsDefined(typeof(IdentifierType), type))
        {
            throw new ArgumentException($"Invalid IdentifierType: {type}");
        }
    }

    private static string CreateKey(IndexEntry entry) =>
        $"{(int)entry.Type}-{entry.Key}-{entry.Value}";

    private void ExpireIndexEntries(object? state)
    {
        var expireKeys = 
            IndexEntries.Where(k => DateTime.Now - k.Value.Time > SlidingExpiration)
                .Select(k => k.Key)
                .ToList();
        foreach (var key in expireKeys)
        {
            IndexEntries.Remove(key, out _);
        }
    }
}
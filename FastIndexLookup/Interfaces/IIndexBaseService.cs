using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace IndexService;

public interface IIndexBaseService
{
    void LoadIndexEntries(IEnumerable<IndexEntry> entries);
    void Upsert(IndexEntry entry);
    bool TryGetValue(string key, [NotNullWhen(true)] out IndexEntry? result);
    ConcurrentDictionary<string, Cache> GetIndexEntries();
}
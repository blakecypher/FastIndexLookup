using System.Diagnostics.CodeAnalysis;

namespace IndexService;

public interface IIndexService
{
    void LoadIndexEntries(IEnumerable<IndexEntry> entries);
    bool TryGetValue(string key, [NotNullWhen(true)] out IndexEntry? result);
    void AddOrUpdate(IndexEntry entry);
}
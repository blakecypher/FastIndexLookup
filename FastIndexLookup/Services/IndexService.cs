using System.Diagnostics.CodeAnalysis;

namespace IndexService;

public class IndexService(IIndexBaseService serviceBase) : IIndexService
{
    public void LoadIndexEntries(IEnumerable<IndexEntry> entries)
    {
        serviceBase.LoadIndexEntries(entries);
    }

    public bool TryGetValue(string key, [NotNullWhen(true)] out IndexEntry? result)
    {
        return serviceBase.TryGetValue(key, out result);
    }

    public void AddOrUpdate(IndexEntry entry)
    {
        serviceBase.Upsert(entry);
    }
}
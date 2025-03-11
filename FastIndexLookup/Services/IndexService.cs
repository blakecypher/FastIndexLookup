using System.Diagnostics.CodeAnalysis;

namespace IndexService;

public class IndexService
{
    private readonly IndexBaseService _serviceBase;

    public IndexService(IndexBaseService serviceBase)
    {
        _serviceBase = serviceBase;
    }

    public void LoadIndexEntries(IEnumerable<IndexEntry> entries)
    {
        _serviceBase.LoadIndexEntries(entries);
    }

    public bool TryGetValue(string key, [NotNullWhen(true)] out IndexEntry? result)
    {
        return _serviceBase.TryGetValue(key, out result);
    }

    public void AddOrUpdate(IndexEntry entry)
    {
        _serviceBase.Upsert(entry);
    }
}
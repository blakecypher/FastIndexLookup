using System.Diagnostics.CodeAnalysis;

namespace IndexService;

public class OrleansIndexService : Grain
{
    private readonly IndexBaseService _serviceBase;

    public OrleansIndexService(IndexBaseService serviceBase)
    {
        _serviceBase = serviceBase;
    }

    public Task LoadIndexEntries(IEnumerable<IndexEntry> entries)
    {
        _serviceBase.LoadIndexEntries(entries);
        return Task.CompletedTask;
    }

    public Task<bool> TryGetValue(string key, [NotNullWhen(true)] out IndexEntry? result)
    {
        var success = _serviceBase.TryGetValue(key, out result);
        return Task.FromResult(success);
    }

    public Task AddOrUpdate(IndexEntry entry)
    {
        _serviceBase.Upsert(entry);
        return Task.CompletedTask;
    }
}
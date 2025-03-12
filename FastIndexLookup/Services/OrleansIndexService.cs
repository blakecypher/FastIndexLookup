namespace IndexService;

public class OrleansIndexService(IIndexBaseService serviceBase) : Grain, IOrleansIndexService
{
    public Task LoadIndexEntries(IEnumerable<IndexEntry> entries)
    {
        serviceBase.LoadIndexEntries(entries);
        return Task.CompletedTask;
    }

    public Task<(bool Success, IndexEntry? Result)> TryGetValue(string key)
    {
        var success = serviceBase.TryGetValue(key, out var result);
        return Task.FromResult((success, result));
    }

    public Task AddOrUpdate(IndexEntry entry)
    {
        serviceBase.Upsert(entry);
        return Task.CompletedTask;
    }
}
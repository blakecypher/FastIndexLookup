namespace IndexService;

public interface IOrleansIndexService : IGrainWithStringKey
{
    Task LoadIndexEntries(IEnumerable<IndexEntry> entries);
    Task<(bool Success, IndexEntry? Result)> TryGetValue(string key);
    Task AddOrUpdate(IndexEntry entry);
}
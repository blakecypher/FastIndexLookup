using System.Diagnostics.CodeAnalysis;

namespace IndexService;

public class IndexBaseService
{
    private readonly Dictionary<string, IndexEntry> _indexEntries = new();

    public void Upsert(IndexEntry entry)
    {
        ValidateType(entry.Type);
        if (_indexEntries.TryGetValue(CreateKey(entry), out var existing))
        {
            var mergedIds = new HashSet<Guid>(existing.IDs);
            foreach (var id in entry.IDs) mergedIds.Add(id);
            _indexEntries[CreateKey(entry)] = existing with { IDs = mergedIds.ToArray() };
        }
        else
        {
            _indexEntries[CreateKey(entry)] = entry;
        }
    }

    public bool TryGetValue(string key, [NotNullWhen(true)] out IndexEntry? result)
    {
        return _indexEntries.TryGetValue(key, out result);
    }

    public void LoadIndexEntries(IEnumerable<IndexEntry> entries)
    {
        foreach (var entry in entries)
        {
            Upsert(entry);
        }
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
}
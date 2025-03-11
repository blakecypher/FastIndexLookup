using Xunit;

namespace IndexService.Tests;

public class IndexServiceTests
{
    private readonly IndexService _service;

    public IndexServiceTests()
    {
        var serviceBase = new IndexBaseService();
        _service = new IndexService(serviceBase);
    }

    [Fact]
    public void LoadIndexEntries_ShouldStoreAllEntries()
    {
        var entries = new List<IndexEntry>
        {
            new(IdentifierType.Type1, "Name1", "Value1", [Guid.NewGuid()]),
            new(IdentifierType.Type2, "Name2", "Value2", [Guid.NewGuid()])
        };

        _service.LoadIndexEntries(entries);

        foreach (var entry in entries)
        {
            var key = $"{(int)entry.Type}-{entry.Key}-{entry.Value}";
            _service.TryGetValue(key, out var result);

            Assert.Equal(entry, result);
        }
    }

    [Fact]
    public void AddOrUpdate_ShouldCallUpsert()
    {
        var entry = new IndexEntry(IdentifierType.Type1, "Name1", "Value1", [Guid.NewGuid()]);

        _service.AddOrUpdate(entry);

        Assert.True(_service.TryGetValue("0-Name1-Value1", out var result));
        Assert.Equal(entry, result);
    }
}
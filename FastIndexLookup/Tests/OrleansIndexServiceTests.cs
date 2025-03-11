using Xunit;

namespace IndexService.Tests;

public class OrleansIndexServiceTests
{
    private readonly OrleansIndexService _service;

    public OrleansIndexServiceTests()
    {
        var baseService = new IndexBaseService();
        _service = new OrleansIndexService(baseService);
    }

    [Fact]
    public async Task LoadIndexEntries_ShouldStoreAllEntriesAsync()
    {
        var entries = new List<IndexEntry>
        {
            new(IdentifierType.Type1, "Name1", "Value1", [Guid.NewGuid()]),
            new(IdentifierType.Type2, "Name2", "Value2", [Guid.NewGuid()])
        };

        await _service.LoadIndexEntries(entries);

        foreach (var entry in entries)
        {
            var key = $"{(int)entry.Type}-{entry.Key}-{entry.Value}";
            var success = await _service.TryGetValue(key, out _);
            Assert.True(success);
        }
    }

    [Fact]
    public async Task AddOrUpdate_ShouldCallUpsertAsync()
    {
        var entry = new IndexEntry(IdentifierType.Type1, "Name1", "Value1", [Guid.NewGuid()]);

        await _service.AddOrUpdate(entry);

        var success = await _service.TryGetValue("0-Name1-Value1", out var result);
        Assert.True(success);
        Assert.Equal(entry.Value, result?.Value);
    }

    [Fact]
    public async Task TryGetValue_ShouldReturnFalseForMissingKeyAsync()
    {
        var success = await _service.TryGetValue("missing-key", out _);
        Assert.False(success);
    }
}
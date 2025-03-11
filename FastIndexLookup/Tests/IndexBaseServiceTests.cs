using Xunit;

namespace IndexService.Tests;

public class IndexBaseServiceTests
{
    private readonly IndexBaseService _baseService;

    public IndexBaseServiceTests()
    {
        _baseService = new IndexBaseService();
    }

    [Fact]
    public void Upsert_ShouldAddNewEntry()
    {
        var entry = new IndexEntry(IdentifierType.Type1, "Name1", "Value1", [Guid.NewGuid()]);

        _baseService.Upsert(entry);

        Assert.True(_baseService.TryGetValue("0-Name1-Value1", out var result));
        Assert.Equal(entry, result);
    }

    [Fact]
    public void Upsert_ShouldMergeIDsForExistingEntry()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var entry1 = new IndexEntry(IdentifierType.Type1, "Name1", "Value1", [id1]);
        var entry2 = new IndexEntry(IdentifierType.Type1, "Name1", "Value1", [id2]);

        _baseService.Upsert(entry1);
        _baseService.Upsert(entry2);

        Assert.True(_baseService.TryGetValue("0-Name1-Value1", out var result));
        Assert.Equal(2, result.IDs.Length);
        Assert.Contains(id1, result.IDs);
        Assert.Contains(id2, result.IDs);
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalseForMissingKey()
    {
        Assert.False(_baseService.TryGetValue("missing-key", out _));
    }

    [Fact]
    public void ValidateType_ShouldThrowForInvalidIdentifierType()
    {
        var invalidEntry = new IndexEntry((IdentifierType)99, "Invalid", "Type", [Guid.NewGuid()]);

        Assert.Throws<ArgumentException>(() => _baseService.Upsert(invalidEntry));
    }
}
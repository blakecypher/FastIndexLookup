using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace IndexService;

public class IndexBaseServiceTests
{
    private readonly Mock<IConfigurationBootStrapper> bootStrapper = new();

    public IndexBaseServiceTests()
    {
        var mockConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "CacheSettings:SlidingExpiration", "00:10:00" }
            })
            .Build();
        bootStrapper.Setup(b => b.GetConfig()).Returns(mockConfig);
    }

    [Fact]
    public void Upsert_ShouldAddNewEntry()
    {
        var baseService = new Mock<IIndexBaseService>();
        var entry = new IndexEntry(IdentifierType.Trainers, "Name1", "Value1", [Guid.NewGuid()]);
        const string key = "0-Name1-Value1";
        var indexEntries = new ConcurrentDictionary<string, Cache>();

        baseService.Setup(s => s.TryGetValue(key, out It.Ref<IndexEntry?>.IsAny))
            .Returns((string _, out IndexEntry? callBackResult) =>
            {
                var success = indexEntries.TryGetValue(key, out var cache);
                callBackResult = success ? cache?.Entry : null;
                return success;
            });

        baseService.Setup(s => s.Upsert(It.IsAny<IndexEntry>()))
            .Callback((IndexEntry e) =>
            {
                indexEntries[key] = new Cache(e, DateTime.Now);
            });
        
        baseService.Object.Upsert(entry);
        
        var success = baseService.Object.TryGetValue(key, out var result);
        
        Assert.True(indexEntries.ContainsKey(key));
        Assert.True(success);
        Assert.Equal(entry, result);
    }

    [Fact]
    public void Upsert_ShouldMergeIDsForExistingEntry()
    {
        var baseService = new Mock<IIndexBaseService>();
        const string key = "0-Name1-Value1";
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var entry1 = new IndexEntry(IdentifierType.Trainers, "Name1", "Value1", [id1]);
        var entry2 = new IndexEntry(IdentifierType.Trainers, "Name1", "Value1", [id2]);
        
        var indexEntries = new ConcurrentDictionary<string, Cache>();

        baseService.Setup(s => s.TryGetValue(key, out It.Ref<IndexEntry?>.IsAny))
            .Returns((string _, out IndexEntry? callBackResult) =>
            {
                var success = indexEntries.TryGetValue(key, out var cache);
                callBackResult = success ? cache?.Entry : null;
                return success;
            });

        baseService.Setup(s => s.Upsert(It.IsAny<IndexEntry>()))
            .Callback((IndexEntry c) =>
            {
                if (indexEntries.TryGetValue(key, out var cache))
                {
                    var mergedIds = new HashSet<Guid>(cache.Entry.IDs);
                    foreach (var id in c.IDs) mergedIds.Add(id);
                    indexEntries[key] = new Cache(cache.Entry with { IDs = mergedIds.ToArray()}, DateTime.Now);
                }
                else
                {
                    indexEntries[key] = new Cache(c, DateTime.Now);
                }
            });

        baseService.Object.Upsert(entry1);
        baseService.Object.Upsert(entry2);

        Assert.True(baseService.Object.TryGetValue("0-Name1-Value1", out var result));
        Assert.Equal(2, result.IDs.Length);
        Assert.Contains(id1, result.IDs);
        Assert.Contains(id2, result.IDs);
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalseForMissingKey()
    {
        var baseService = new Mock<IIndexBaseService>();
        Assert.False(baseService.Object.TryGetValue("missing-key", out _));
    }

    [Fact]
    public void ValidateType_ShouldThrowForInvalidIdentifierType()
    {
        var invalidEntry = new IndexEntry((IdentifierType)99, "Invalid", "Type", [Guid.NewGuid()]);
        var baseService = new IndexBaseService(bootStrapper.Object);

        Assert.Throws<ArgumentException>(() => baseService.Upsert(invalidEntry));
    }
}
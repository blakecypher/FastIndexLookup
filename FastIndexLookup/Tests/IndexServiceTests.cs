using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace IndexService;

public class IndexServiceTests
{
    private readonly ITestOutputHelper testOutputHelper;
    private readonly Mock<IConfigurationBootStrapper> bootStrapper = new();
    private readonly IIndexService service;
    private const int loadCount = 10000;

    public IndexServiceTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
        var mockConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "CacheSettings:SlidingExpiration", "00:10:00" }
            })
            .Build();
        bootStrapper.Setup(b => b.GetConfig()).Returns(mockConfig);
        IIndexBaseService baseService = new IndexBaseService(bootStrapper.Object);
        service = new IndexService(baseService);
    }
    [Fact]
    public void LoadIndexEntries_ShouldStoreAllEntries()
    {
        var entries = new List<IndexEntry>
        {
            new(IdentifierType.Trainers, "Name1", "Value1", [Guid.NewGuid()]),
            new(IdentifierType.Tracksuits, "Name2", "Value2", [Guid.NewGuid()])
        };

        service.LoadIndexEntries(entries);

        foreach (var entry in entries)
        {
            var key = $"{(int)entry.Type}-{entry.Key}-{entry.Value}";
            service.TryGetValue(key, out var result);

            Assert.Equal(entry, result);
        }
    }

    [Fact]
    public void AddOrUpdate_ShouldCallUpsert()
    {
        var entry = new IndexEntry(IdentifierType.Trainers, "Name1", "Value1", [Guid.NewGuid()]);

        service.AddOrUpdate(entry);

        Assert.True(service.TryGetValue("0-Name1-Value1", out var result));
        Assert.Equal(entry, result);
    }
    
    [Fact]
    public void LoadTest()
    {
        var entries = TestHelpers.LoadTesting(loadCount);
            
        var before = GC.GetTotalMemory(true);
        service.LoadIndexEntries(entries);
        var after = GC.GetTotalMemory(true);
            
        var bytesPerEntry = (after - before) / (double)loadCount;
        testOutputHelper.WriteLine($"Memory usage per entry: {bytesPerEntry:F2} bytes");
        Assert.True(bytesPerEntry < 200, $"Memory usage per entry too high: {bytesPerEntry} bytes");
    }
}
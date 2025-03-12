using Orleans.TestingHost;
using Xunit;
using Xunit.Abstractions;

namespace IndexService;

public class OrleansIndexServiceTests(ITestOutputHelper testOutputHelper) : IAsyncLifetime
{
    private TestCluster cluster;
    private IClusterClient client;

    private const int loadCount = 10000;
    
    public async Task InitializeAsync()
    {
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<SiloBuilder>();
        
        cluster = builder.Build();
        await cluster.DeployAsync();

        client = cluster.Client;
    }

    [Fact]
    public async Task LoadIndexEntries_ShouldStoreAllEntriesAsync()
    {
        var key = "0-Name1-Value1";
        var grain = client.GetGrain<IOrleansIndexService>(key);
        var entries = new List<IndexEntry>
        {
            new(IdentifierType.Trainers, "Name1", "Value1", [Guid.NewGuid()]),
            new(IdentifierType.Tracksuits, "Name2", "Value2", [Guid.NewGuid()])
        };

        await grain.LoadIndexEntries(entries);

        foreach (var entry in entries)
        {
            key = $"{(int)entry.Type}-{entry.Key}-{entry.Value}";
            var result = await grain.TryGetValue(key);
            Assert.True(result.Success);
        }

        await DisposeAsync();
    }

    [Fact]
    public async Task AddOrUpdate_ShouldCallUpsertAsync()
    {
        const string key = "0-Name1-Value1";
        var grain = client.GetGrain<IOrleansIndexService>(key);
        var entry = new IndexEntry(IdentifierType.Trainers, "Name1", "Value1", [Guid.NewGuid()]);

        await grain.AddOrUpdate(entry);

        var result = await grain.TryGetValue(key);
        Assert.True(result.Success);

        await DisposeAsync();
    }

    [Fact]
    public async Task TryGetValue_ShouldReturnFalseForMissingKeyAsync()
    {
        var grain = client.GetGrain<IOrleansIndexService>("missing-key");
        var result = await grain.TryGetValue("missing-key");
        Assert.False(result.Success);

        await DisposeAsync();
    }
    
    [Fact]
    public void LoadTest()
    {
        const string key = "0-Name1-Value1";
        var grain = client.GetGrain<IOrleansIndexService>(key);
        var entries = TestHelpers.LoadTesting(loadCount);
            
        var before = GC.GetTotalMemory(true);
        grain.LoadIndexEntries(entries);
        var after = GC.GetTotalMemory(true);
            
        var bytesPerEntry = (after - before) / (double)loadCount;
        testOutputHelper.WriteLine($"Memory usage per entry: {bytesPerEntry:F2} bytes");
        Assert.True(bytesPerEntry < 200, $"Memory usage per entry too high: {bytesPerEntry} bytes");
    }

    public async Task DisposeAsync()
    {
        // Teardown
        await cluster.StopAllSilosAsync();
    }
}
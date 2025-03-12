using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.TestingHost;

namespace IndexService;

public class SiloBuilder : ISiloConfigurator
{
    private readonly Mock<IConfigurationBootStrapper> bootStrapper = new();
    public void Configure(ISiloBuilder siloBuilder)
    {
        var mockConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "CacheSettings:SlidingExpiration", "00:10:00" }
            })
            .Build();
        bootStrapper.Setup(b => b.GetConfig()).Returns(mockConfig);
        siloBuilder.AddMemoryGrainStorageAsDefault();
        siloBuilder.ConfigureServices(services =>
        {
            services.AddSingleton(bootStrapper.Object);
            services.AddSingleton<IIndexBaseService>(sp =>
                new IndexBaseService(sp.GetRequiredService<IConfigurationBootStrapper>()));
        });
    }
}
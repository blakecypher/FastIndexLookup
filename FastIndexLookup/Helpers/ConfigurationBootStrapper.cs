using Microsoft.Extensions.Configuration;

namespace IndexService;

public class ConfigurationBootStrapper : IConfigurationBootStrapper
{
    public IConfiguration GetConfig()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appSettings.json", true, true)
            .Build();
    }
}
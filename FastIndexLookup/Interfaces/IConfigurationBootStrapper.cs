using Microsoft.Extensions.Configuration;

namespace IndexService;

public interface IConfigurationBootStrapper
{
    IConfiguration GetConfig();
}
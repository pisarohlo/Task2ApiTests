using Microsoft.Extensions.Configuration;

namespace ApiTests.Configuration
{
    public static class ConfigurationLoader
    {
        // configure appsettings file
        private static readonly Lazy<IConfigurationRoot> _config = new Lazy<IConfigurationRoot>(() =>
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build());

        public static Settings Settings => _config.Value.GetSection("Settings").Get<Settings>();
    }
}

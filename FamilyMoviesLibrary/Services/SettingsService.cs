using Microsoft.Extensions.Configuration;

namespace FamilyMoviesLibrary.Services;

public static class SettingsService
{
    public static IConfigurationRoot Configuration {
        get
        {
            return _configurationRoot;
        }
    }
    private static IConfigurationRoot _configurationRoot = null!;

    public static void Initialization()
    {
        var configuration =  new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json");
            
        _configurationRoot = configuration.Build();
    }

    public static string? GetConnectionString(string connectionName)
    {
        return _configurationRoot[$"ConnectionStrings:{connectionName}"];
    }

    public static string? GetBotToken()
    {
        return _configurationRoot["Telegram:Token"];
    }
}
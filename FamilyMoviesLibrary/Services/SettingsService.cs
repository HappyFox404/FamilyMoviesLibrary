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
    
    public static string GetDefaultConnectionString()
    {
        var connection = _configurationRoot["ConnectionStrings:DefaultConnection"];
        if (connection != default)
            return connection;
        throw new NullReferenceException("Не указана строка подключения по умолчанию");
    }

    public static string? GetBotToken()
    {
        var token = _configurationRoot["Telegram:Token"];
        if (token != default)
            return token;
        throw new NullReferenceException("Не указан токен требуемый для работы!");
    }

    public static string GetUnofficalKinopoiskToken()
    {
        var token = _configurationRoot["KinopoiskUnoffical:Token"];
        if (token != default)
            return token;
        throw new NullReferenceException("Не указан токен требуемый для работы!");
    }
    
    public static string GetUnofficalKinopoiskUrl()
    {
        var token = _configurationRoot["KinopoiskUnoffical:KinopoiskUnofficalUrl"];
        if (token != default)
            return token;
        throw new NullReferenceException("Не указан url KinopoiskUnoffical!");
    }
    
    public static string GetUnofficalKinopoiskGenresMethod()
    {
        var token = _configurationRoot["KinopoiskUnoffical:Genres"];
        if (token != default)
            return token;
        throw new NullReferenceException("Не указан url для получения genres!");
    }
    
    public static string GetUnofficalKinopoiskFilmsMethod()
    {
        var token = _configurationRoot["KinopoiskUnoffical:Films"];
        if (token != default)
            return token;
        throw new NullReferenceException("Не указан url для получения films!");
    }
}
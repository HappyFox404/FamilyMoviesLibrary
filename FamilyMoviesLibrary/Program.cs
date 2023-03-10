using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Models.Settings;
using FamilyMoviesLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var services = new ServiceCollection()
    .AddLogging(builder =>
    {
        builder.AddConsole();
        builder.AddDebug();
    })
    .Configure<TelegramSettings>(configuration.GetSection("Telegram"))
    .Configure<KinpoiskUnofficalSettings>(configuration.GetSection("KinopoiskUnoffical"))
    .AddDbContext<FamilyMoviesLibraryContext>( 
        opts => opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")))
    .AddSingleton<IFilmService, FilmService>()
    .AddSingleton<IStorageService, StorageService>()
    .AddSingleton<IBotService, BotService>()
    .BuildServiceProvider();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
await services.GetService<IBotService>()!.StartBot();
    
    
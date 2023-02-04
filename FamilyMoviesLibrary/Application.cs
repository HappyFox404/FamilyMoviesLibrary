using FamilyMoviesLibrary.ApplicationCommands;
using FamilyMoviesLibrary.Helpers;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Services;

namespace FamilyMoviesLibrary;

public class Application
{
    public static Application? Instance;
    
    private bool _isRun = false;
    private bool _isErrorInitialization = false;

    private readonly IEnumerable<IApplicationCommand> _commands;
    private readonly IApplicationCommand _defaultCommand;

    public Application()
    {
        _commands = SystemHelper.GetApplicationCommands();
        _defaultCommand = SystemHelper.GetApplicationDefaultCommand();
        SettingsService.Initialization();
        try
        {
            StorageService.LoadKinopoiskUnofficalFilterData();
        }
        catch
        {
            _isErrorInitialization = true;
        }
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        Instance = this;
    }

    public async Task Start()
    {
        if (_isErrorInitialization)
        {
            Console.WriteLine("Error for initialization data, application not started!");
            return;
        }
        _isRun = true;
        
        var botToken = SettingsService.GetBotToken();
        if (botToken != default)
        {
            BotService botService = new BotService(botToken, ListenCommands);
            await botService.StartBot();
        }
        else
        {
            Console.WriteLine("Bot token not initialized");
        }
        
        Console.WriteLine("End Work!");
        Console.ReadLine();
    }

    public void Stop()
    {
        _isRun = false;
    }

    private void ListenCommands()
    {
        Console.WriteLine("Wait input Commands...");
        while (_isRun)
        {
            var readCommand = Console.ReadLine();
            if (readCommand != default)
            {
                bool foundCommand = false;
                foreach (var command in _commands)
                {
                    if (command.IsNeedCommand(readCommand))
                    {
                        foundCommand = true;
                        command.ExecuteCommand(readCommand);
                        break;
                    }
                }
                if (!foundCommand)
                {
                    _defaultCommand.ExecuteCommand(readCommand);
                }
            }
        }
    }
}
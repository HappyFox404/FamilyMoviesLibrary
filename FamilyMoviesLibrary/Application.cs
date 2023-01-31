using FamilyMoviesLibrary.ApplicationCommands;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Services;

namespace FamilyMoviesLibrary;

public class Application
{
    public static Application? Instance;
    
    private bool _isRun = false;

    private readonly IEnumerable<IApplicationCommand> _commands = new List<IApplicationCommand>()
    {
        new HelpClientCommand()
    };
    private readonly IApplicationCommand _defaultCommand = new DefaultClientCommand();

    public Application()
    {
        SettingsService.Initialization();
        Instance = this;
    }

    public async Task Start()
    {
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
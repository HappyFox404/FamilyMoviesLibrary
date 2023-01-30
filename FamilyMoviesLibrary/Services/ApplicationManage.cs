namespace FamilyMoviesLibrary.Services;

public class ApplicationManage
{
    private bool _isRun = false;

    private const string ExitCommand = "exit";
    
    public ApplicationManage()
    {
        Settings.Initialization();
    }

    public async Task Start()
    {
        _isRun = true;
        
        var botToken = Settings.GetBotToken();
        if (botToken != default)
        {
            InterfaceBot interfaceBot = new InterfaceBot(botToken, ListenCommands);
            await interfaceBot.StartBot();
        }
        else
        {
            Console.WriteLine("Bot token not initialized");
        }
        
        Console.WriteLine("End Work!");
        Console.ReadLine();
    }

    private void ListenCommands()
    {
        Console.WriteLine("Wait input Commands...");
        while (true)
        {
            var command = Console.ReadLine();
            switch (command)
            {
                case ExitCommand:
                {
                    return;
                }
                default:
                {
                    Console.WriteLine("Not command");
                } break;
            }
        }
    }
}
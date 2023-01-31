namespace FamilyMoviesLibrary.Services.Helpers;

public class CommandBuilder
{
    public string Command { get; }
    public List<string> Arguments { get; }
    public bool ValidCommand { get; }

    public CommandBuilder(string command)
    {
        if (command.Contains("\"") == false)
        {
            var values = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (values.Any())
            {
                Command = values.First();
                if (Command.Length > 1)
                {
                    Arguments = values.Skip(1).ToList();
                }
                ValidCommand = true;
            }
            else
            {
                ValidCommand = false;
            }
        }
        else
        {
            
        }
    }
}
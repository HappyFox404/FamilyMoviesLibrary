using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Services.Helpers;

namespace FamilyMoviesLibrary.ApplicationCommands;

public class HelpClientCommand : IApplicationCommand
{
    public bool IsNeedCommand(string command)
    {
        var buildCommand = new CommandBuilder(command);
        if (buildCommand.ValidCommand && 
            buildCommand.Command == "/help")
        {
            return true;
        }
        return false;
    }

    public void ExecuteCommand(string command)
    {
        Console.WriteLine("Command list:");
    }
}
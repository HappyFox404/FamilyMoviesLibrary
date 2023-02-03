using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Models.Atributes;
using FamilyMoviesLibrary.Services;

namespace FamilyMoviesLibrary.ApplicationCommands;

[ApplicationCommand(true)]
public class DefaultClientCommand : IApplicationCommand
{
    public bool IsNeedCommand(string command)
    {
        return true;
    }

    public void ExecuteCommand(string command)
    {
        Console.WriteLine("This command was not found. Enter /help to see list commands.");
    }
}
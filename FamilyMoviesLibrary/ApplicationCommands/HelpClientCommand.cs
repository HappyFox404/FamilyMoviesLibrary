using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Models;
using FamilyMoviesLibrary.Models.Atributes;
using FamilyMoviesLibrary.Services.Helpers;

namespace FamilyMoviesLibrary.ApplicationCommands;

[ApplicationCommand]
public class HelpClientCommand : IApplicationCommand
{
    public bool IsNeedCommand(string command)
    {
        return new CommandBuilder(command).DefinationCommand(ApplicationCommandNames.Help);
    }

    public void ExecuteCommand(string command)
    {
        Console.WriteLine($"Command list: \n/exit - Close application");
    }
}
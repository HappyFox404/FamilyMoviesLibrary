using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Models;
using FamilyMoviesLibrary.Models.Atributes;
using FamilyMoviesLibrary.Services.Helpers;

namespace FamilyMoviesLibrary.ApplicationCommands;

[ApplicationCommand]
public class ExitClientCommand : IApplicationCommand
{
    public bool IsNeedCommand(string command)
    {
        return new CommandBuilder(command).DefinationCommand(ApplicationCommandNames.Exit);
    }

    public void ExecuteCommand(string command)
    {
        var buildCommand = new CommandBuilder(command);
        if (buildCommand.ValidCommand)
        {
            Application.Instance?.Stop();
        }
    }
}
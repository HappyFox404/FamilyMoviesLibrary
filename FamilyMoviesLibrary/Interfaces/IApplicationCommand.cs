namespace FamilyMoviesLibrary.Interfaces;

public interface IApplicationCommand
{
    bool IsNeedCommand(string command);
    void ExecuteCommand(string command);
}
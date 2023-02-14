using System.Reflection;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Models.Atributes;

namespace FamilyMoviesLibrary.Helpers;

public static class SystemHelper
{
    public static List<IBotCommand> GetBotCommands()
    {
        List<IBotCommand> commands = new();

        Assembly currentBuild = Assembly.GetExecutingAssembly();

        var allTypes = currentBuild.DefinedTypes;

        foreach (var oneType in allTypes.Where(x => x.GetCustomAttribute(typeof(BotCommandAttribute), false) != default).ToList())
        {
            var attribute = oneType.GetCustomAttribute(typeof(BotCommandAttribute), false);
            if (oneType.GetInterface(nameof(IBotCommand)) != default)
            {
                if (attribute is BotCommandAttribute botCommandAttribute)
                {
                    if (botCommandAttribute.IsDefault == false)
                    {
                        commands.Add((IBotCommand)Activator.CreateInstance(oneType));
                    }
                }
            }
        }
        
        return commands;
    }
    
    public static IBotCommand GetBotDefaultCommand()
    {
        IBotCommand command = null;

        Assembly currentBuild = Assembly.GetExecutingAssembly();

        var allTypes = currentBuild.DefinedTypes;

        foreach (var oneType in allTypes.Where(x => x.GetCustomAttribute(typeof(BotCommandAttribute), false) != default).ToList())
        {
            var attribute = oneType.GetCustomAttribute(typeof(BotCommandAttribute), false);
            if (oneType.GetInterface(nameof(IBotCommand)) != default)
            {
                if (attribute is BotCommandAttribute botCommandAttribute)
                {
                    if (botCommandAttribute.IsDefault)
                    {
                        command = (IBotCommand)Activator.CreateInstance(oneType);
                    }
                }
            }
        }
        
        return command;
    }
}
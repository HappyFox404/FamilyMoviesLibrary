using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Context.ContextQuery;
using FamilyMoviesLibrary.Models;
using FamilyMoviesLibrary.Models.Atributes;
using FamilyMoviesLibrary.Models.Exception;
using FamilyMoviesLibrary.Models.Extension;
using FamilyMoviesLibrary.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FamilyMoviesLibrary.BotCommands;

[BotCommand]
public class GroupExitCommand : IBotCommand
{
    public bool IsNeedCommand(string command)
    {
        
        return new CommandBuilder(command).DefinationCommand(BotCommandNames.GroupExit);
    }

    public async Task ExecuteCommand(FamilyMoviesLibraryContext context, string command, TelegramBotClient client, Update update, 
        IServiceProvider collection, CancellationToken cancellationToken)
    {
        var buildCommand = new CommandBuilder(command);
        if (buildCommand.ValidCommand)
        {
            User user = update.GetUser();
            ChatId chatId = update.GetChatId();
            var userData = await context.GetUser(user.Id);
            
            string message = "Вас нет в библиотеке.";
            if (userData?.Group != default)
            {
                await context.ExitGroup(user.Id);
                message = "Вы успешно вышли из библиотеки";
            }
            await context.SetMessage(user.Id, command);
            await client.SendDefaultMessage(
                message,
                chatId, cancellationToken);
        }
    }
}
using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Models;
using FamilyMoviesLibrary.Models.Atributes;
using FamilyMoviesLibrary.Models.Exception;
using FamilyMoviesLibrary.Services.Helpers;
using Microsoft.EntityFrameworkCore;
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

    public async Task ExecuteCommand(FamilyMoviesLibraryContext context, string command, TelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        var buildCommand = new CommandBuilder(command);
        if (buildCommand.ValidCommand)
        {
            ChatId? chatId = TelegramHelper.GetChatId(update);
            User? user = TelegramHelper.GetUser(update);
            
            if (user == default)
                return;

            var userData = context.Users.Include(x => x.Group).FirstOrDefault(x => x.TelegramId == user.Id);
            string message = String.Empty;
            if (userData?.Group != default)
            {
                try
                {
                    await context.ExitGroup(user.Id);
                    message = "Вы успешно вышли из библиотеки";
                }
                catch (ControllException exception)
                {
                    message = $"{exception.NormalMessage}";
                }
            }
            else
            {
                message = "Вы не находитесь в библиотеке!";
            }
            await context.SetMessage(user.Id, command);
            await client.SendDefaultMessage(
                message,
                chatId, cancellationToken);
        }
    }
}
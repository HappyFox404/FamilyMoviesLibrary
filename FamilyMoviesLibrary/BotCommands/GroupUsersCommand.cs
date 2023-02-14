using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Context.ContextQuery;
using FamilyMoviesLibrary.Context.Models;
using FamilyMoviesLibrary.Models;
using FamilyMoviesLibrary.Models.Atributes;
using FamilyMoviesLibrary.Models.Exception;
using FamilyMoviesLibrary.Models.Extension;
using FamilyMoviesLibrary.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = Telegram.Bot.Types.User;

namespace FamilyMoviesLibrary.BotCommands;

[BotCommand]
public class GroupUsersCommand : IBotCommand
{
    public bool IsNeedCommand(string command)
    {
        return new CommandBuilder(command).DefinationCommand(BotCommandNames.GroupUsers);
    }

    public async Task ExecuteCommand(FamilyMoviesLibraryContext context, string command, TelegramBotClient client, Update update, 
        IServiceProvider collection,
        CancellationToken cancellationToken)
    {
        var buildCommand = new CommandBuilder(command);
        if (buildCommand.ValidCommand)
        {
            User user = update.GetUser();
            ChatId chatId = update.GetChatId();

            var userData = await context.GetUser(user.Id);
            Group? group = await context.Groups.FirstOrDefaultAsync(x => x.Id == userData.GroupId);
            
            string message = "Вас нет в библиотеке.";
            if (group != default)
            {
                var usersInGroup = context.Users.Where(x => x.GroupId == group.Id)
                    .Select(x => x.TelegramUserName).ToList();
                if (usersInGroup.Any() == false)
                {
                    message = $"В бибилотеке ({group.Name}) нет участников и скорее всего произошла ошибка.";
                }
                else
                {
                    message = $"В библиотеке ({group.Name}) состоят:\n{String.Join("\n", usersInGroup)}";
                }
            }
            await context.SetMessage(user.Id, command);
            await client.SendDefaultMessage(
                message,
                chatId, cancellationToken);
        }
    }
}
using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Context.Models;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Models;
using FamilyMoviesLibrary.Models.Atributes;
using FamilyMoviesLibrary.Models.Exception;
using FamilyMoviesLibrary.Services.Helpers;
using Microsoft.EntityFrameworkCore;
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
        CancellationToken cancellationToken)
    {
        var buildCommand = new CommandBuilder(command);
        if (buildCommand.ValidCommand)
        {
            ChatId? chatId = TelegramHelper.GetChatId(update);
            User? user = TelegramHelper.GetUser(update);
            
            if (user == default)
                return;

            var userData = await context.Users.FirstOrDefaultAsync(x => x.TelegramId == user.Id);
            Group? group = null;
            if (userData != default)
            {
                group = await context.Groups.FirstOrDefaultAsync(x => x.Id == userData.GroupId);
            }
            string message = String.Empty;
            if (group != default)
            {
                var usersInGroup = context.Users.Where(x => x.GroupId == group.Id)
                    .Select(x => x.TelegramUserName).ToList();
                if (usersInGroup.Any() == false)
                {
                    message = $"В группе ({group.Name}) нет участников и скорее всего произошла ошибка";
                }
                else
                {
                    message = $"В группе ({group.Name}) состоят:\n{String.Join("\n", usersInGroup)}";
                }
            }
            else
            {
                message = "Вы не находитесь в группе!";
            }
            await context.SetMessage(user.Id, command);
            await client.SendDefaultMessage(
                message,
                chatId, cancellationToken);
        }
    }
}
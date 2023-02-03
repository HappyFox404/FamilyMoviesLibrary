using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Context.Models;
using FamilyMoviesLibrary.Helpers;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Models.Exception;
using FamilyMoviesLibrary.Services;
using FamilyMoviesLibrary.Services.Helpers;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Message = Telegram.Bot.Types.Message;
using User = Telegram.Bot.Types.User;

namespace FamilyMoviesLibrary.BotCommands;

public class GroupCreateCommand : IBotCommand
{
    public bool IsNeedCommand(string command)
    {
        var buildCommand = new CommandBuilder(command);
        if (buildCommand.ValidCommand && 
            buildCommand.Command == "/group-create")
        {
            return true;
        }
        return false;
    }

    public async Task ExecuteCommand(string command, TelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        var buildCommand = new CommandBuilder(command);
        if (buildCommand.ValidCommand)
        {
            ChatId? chatId = TelegramHelper.GetChatId(update);
            User? user = TelegramHelper.GetUser(update);

            if (user == default)
                return;

            if (buildCommand.ContainsContinueKey() == false)
            {
                await client.SendDefaultMessage(
                    "Введите название группы:",
                    chatId, cancellationToken);
                await DatabaseHelper.SetMessage(user.Id, command, true);
            }
            else
            {
                string continueArgument = buildCommand.GetContinueValue();
                try
                {
                    await DatabaseHelper.CreateGroup(user.Id, continueArgument);
                }
                catch (ControllException exception)
                {
                    await client.SendDefaultMessage(
                        exception.NormalMessage,
                        chatId, cancellationToken);
                    return;
                }
                await client.SendDefaultMessage(
                    "Группа успешно создана. Также я Вас туда добавил",
                    chatId, cancellationToken);
                await DatabaseHelper.SetMessage(user.Id, command);
            }
        }
    }
}
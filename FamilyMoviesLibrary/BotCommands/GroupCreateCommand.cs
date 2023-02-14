using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Context.Models;
using FamilyMoviesLibrary.Helpers;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Models;
using FamilyMoviesLibrary.Models.Atributes;
using FamilyMoviesLibrary.Models.Exception;
using FamilyMoviesLibrary.Models.Extension;
using FamilyMoviesLibrary.Services;
using FamilyMoviesLibrary.Services.Helpers;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Message = Telegram.Bot.Types.Message;
using User = Telegram.Bot.Types.User;

namespace FamilyMoviesLibrary.BotCommands;

[BotCommand]
public class GroupCreateCommand : IBotCommand
{
    public bool IsNeedCommand(string command)
    {
        return new CommandBuilder(command).DefinationCommand(BotCommandNames.GroupCreate);
    }

    public async Task ExecuteCommand(FamilyMoviesLibraryContext context, string command, TelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        var buildCommand = new CommandBuilder(command);
        if (buildCommand.ValidCommand)
        {
            User user = update.GetUser();
            ChatId chatId = update.GetChatId();

            if (buildCommand.ContainsContinueKey() == false)
            {
                await context.SetMessage(user.Id, command, true);
                await client.SendDefaultMessage(
                    "Введите название библиотеки:",
                    chatId, cancellationToken);
            }
            else
            {
                string continueArgument = buildCommand.GetContinueValue();
                await context.CreateGroup(user.Id, continueArgument);
                await context.SetMessage(user.Id, command);
                await client.SendDefaultMessage(
                    "Библиотека успешно создана. Также я Вас туда добавил",
                    chatId, cancellationToken);
            }
        }
    }
}
using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Context.Models;
using FamilyMoviesLibrary.Helpers;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Services;
using FamilyMoviesLibrary.Services.Helpers;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
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

            if (chatId != default && user != default)
            {
                Message sendMessage = await client.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Введите название группы",
                    cancellationToken: cancellationToken);
            }
        }
    }
}
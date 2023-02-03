using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Helpers;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Services;
using FamilyMoviesLibrary.Services.Helpers;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyMoviesLibrary.BotCommands;

public class GroupCommand : IBotCommand
{
    public bool IsNeedCommand(string command)
    {
        var buildCommand = new CommandBuilder(command);
        if (buildCommand.ValidCommand && 
            buildCommand.Command == "/group")
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

            using (FamilyMoviesLibraryContext context =
                   FamilyMoviesLibraryContext.CreateContext(SettingsService.GetDefaultConnectionString()))
            {
                var userData = context.Users.Include(x => x.Group).FirstOrDefault(x => x.TelegramId == user.Id);
                InlineKeyboardMarkup inlineKeyboard;
                if (userData?.Group != default)
                {
                    inlineKeyboard = new(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(text: "Выйти из группы",
                                callbackData: $"/group-exit"),
                            InlineKeyboardButton.WithCallbackData(text: "Участники",
                                callbackData: $"/group-users -g:{userData.Group.Id}")
                        }
                    });
                }
                else
                {
                    inlineKeyboard = new(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(text: "Создать группу",
                                callbackData: $"/group-create"),
                            InlineKeyboardButton.WithCallbackData(text: "Найти группу",
                                callbackData: $"/group-search"),
                        }
                    });
                }
                await client.SendDefaultMessage(
                    "Вот что я могу Вам предложить:",
                    chatId, cancellationToken, inlineKeyboard);
                await DatabaseHelper.SetMessage(user.Id, command);
            }
        }
    }
}
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
            if (buildCommand.Arguments.Any() == false)
            {
                await GenerateBaseResponse(client, update, cancellationToken);
            }
        }
    }
    
    private async Task GenerateBaseResponse(TelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        ChatId? chatId = TelegramHelper.GetChatId(update);
        User? user = TelegramHelper.GetUser(update);

        using (FamilyMoviesLibraryContext context =
               FamilyMoviesLibraryContext.CreateContext(SettingsService.GetDefaultConnectionString()))
        {
            if (chatId != default && user != default)
            {
                if (context.Users.Any(x => x.TelegramId == user.Id) == false)
                {
                    await DatabaseHelper.CreateUser(user.Id);
                }

                var userData = context.Users.Include(x => x.Group).FirstOrDefault(x => x.TelegramId == user.Id);
                InlineKeyboardMarkup inlineKeyboard;
                if (userData.Group != default)
                {
                    inlineKeyboard = new(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(text: "Выйти из группы",
                                callbackData: $"/group-exit -u:{userData.Id} -g:{userData.Group.Id}"),
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
                await client.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Вот что я могу Вам предложить:",
                    replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken);
                
            }
        }
    }
}
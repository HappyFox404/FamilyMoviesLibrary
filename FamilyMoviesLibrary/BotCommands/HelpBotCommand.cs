using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Helpers;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Models;
using FamilyMoviesLibrary.Models.Atributes;
using FamilyMoviesLibrary.Models.Exception;
using FamilyMoviesLibrary.Models.Extension;
using FamilyMoviesLibrary.Services.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyMoviesLibrary.BotCommands;

[BotCommand]
public class HelpBotCommand : IBotCommand
{
    public bool IsNeedCommand(string command)
    {
        return new CommandBuilder(command).DefinationCommand(BotCommandNames.Help);
    }

    public async Task ExecuteCommand(FamilyMoviesLibraryContext context, string command, TelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        if (update.Message != default || update.CallbackQuery != default)
        {
            User user = update.GetUser();
            ChatId chatId = update.GetChatId();
            
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "Помощь", callbackData: BotCommandNames.Help),
                    InlineKeyboardButton.WithCallbackData(text: "Библиотека", callbackData: BotCommandNames.Group),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "Фильмы", callbackData: BotCommandNames.Film)
                }
            });
            
            await context.SetMessage(user.Id, command);
            await client.SendDefaultMessage("Список доступных команд:",
                chatId, cancellationToken, inlineKeyboard);
        }
    }
}
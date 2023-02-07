using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Models;
using FamilyMoviesLibrary.Models.Atributes;
using FamilyMoviesLibrary.Models.Exception;
using FamilyMoviesLibrary.Services.Helpers;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyMoviesLibrary.BotCommands;

[BotCommand]
public class FilmCommand : IBotCommand
{
    public bool IsNeedCommand(string command)
    {
        return new CommandBuilder(command).DefinationCommand(BotCommandNames.Film);
    }

    public async Task ExecuteCommand(FamilyMoviesLibraryContext context, string command, TelegramBotClient client, Update update,
        CancellationToken cancellationToken)
    {
        var buildCommand = new CommandBuilder(command);
        if (buildCommand.ValidCommand)
        {
            ChatId chatId = TelegramHelper.GetChatId(update);
            User user = TelegramHelper.GetUser(update);
            var needUser = await context.GetUser(user.Id);

            if (needUser.Group == default)
            {
                throw new ControllException(
                    "Вы не находитесь в библиотеке! Вступите в библиотеку или создайте новую.");
            }

            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Найти фильм для оценки",
                        callbackData: BotCommandNames.SearchFilm),
                    InlineKeyboardButton.WithCallbackData(text: "Предложи фильм",
                        callbackData: BotCommandNames.RecommendFilm)
                }
            });
            
            await context.SetMessage(user.Id, command);
            await client.SendDefaultMessage(
                "Вот что я могу Вам предложить:",
                chatId, cancellationToken, inlineKeyboard);
        }
    }
}
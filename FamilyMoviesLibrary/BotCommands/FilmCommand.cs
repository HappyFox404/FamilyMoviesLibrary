using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Models;
using FamilyMoviesLibrary.Models.Atributes;
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
            ChatId? chatId = TelegramHelper.GetChatId(update);
            User? user = TelegramHelper.GetUser(update);
            
            if (user == default)
                return;
            
            var needUser = await context.Users.Include(x => x.Group).FirstOrDefaultAsync(x => x.TelegramId == user.Id);
            if (needUser == default)
            {
                throw new ArgumentNullException("не найден пользователь");
            }

            if (needUser.Group == default)
            {
                await client.SendDefaultMessage(
                    "Вы не находитесь в библиотеке для начала вступите в библиотеку или создайте новую",
                    chatId, cancellationToken);
                return;
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
using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Helpers;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Models;
using FamilyMoviesLibrary.Models.Atributes;
using FamilyMoviesLibrary.Models.Data;
using FamilyMoviesLibrary.Models.Exception;
using FamilyMoviesLibrary.Services.Helpers;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyMoviesLibrary.BotCommands;

[BotCommand]
public class SearchFilmCommand : IBotCommand
{
    public bool IsNeedCommand(string command)
    {
        return new CommandBuilder(command).DefinationCommand(BotCommandNames.SearchFilm);
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
            
            if (buildCommand.ContainsContinueKey() == false)
            {
                await context.SetMessage(user.Id, command, true);
                await client.SendDefaultMessage(
                    "Введите полное название или часть, искомого фильма:",
                    chatId, cancellationToken);
            }
            else
            {
                string continueArgument = buildCommand.GetContinueValue();

                List<FilmKinopoiskUnofficalModel> films = await new ApiKinopoiskRequestHelper().SearchFilm(continueArgument);

                
                string messageResponse = "ничего не найдено";
                if (films.Any())
                {
                    if (films.Count() > 10)
                    {
                        InlineKeyboardMarkup inlineKeyboard = new(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData(text: "Попробовать снова",
                                    callbackData: BotCommandNames.SearchFilm)
                            }
                        });
                        await context.SetMessage(user.Id, command);
                        await client.SendDefaultMessage(
                            "Слишком много совпадений, попробуйте уточнить запрос.",
                            chatId, cancellationToken, inlineKeyboard);
                    }
                    else
                    {
                        await client.SendDefaultMessage(
                            $"Вот что я нашёл по вашему запросу:\n{String.Join("\n", films.Select(x => $"{x.NameRu} {x.Year}").ToList())}",
                            chatId, cancellationToken);
                        
                        foreach (var filmKinopoiskUnofficalModel in films)
                        {
                            InlineKeyboardMarkup inlineKeyboard = new(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData(text: "Оценить и добавить в просмотренное",
                                        callbackData: $"{BotCommandNames.LikeFilm} -i:{filmKinopoiskUnofficalModel.KinopoiskId}")
                                }
                            });
                            string countries = String.Join(" ",
                                filmKinopoiskUnofficalModel.Countries.Where(x => x.Country != default)
                                    .Select(x => x.Country).ToList());
                            string genres = String.Join(" ",
                                filmKinopoiskUnofficalModel.Genres.Where(x => x.Genre != default)
                                    .Select(x => x.Genre).ToList());
                            await client.SendDefaultMessage(
                                $"{filmKinopoiskUnofficalModel.PosterUrl}\n" +
                                $"{filmKinopoiskUnofficalModel.NameRu} {filmKinopoiskUnofficalModel.Year}\n" +
                                $"Страны: {countries}.\n" +
                                $"Жанры: {genres}\n" +
                                $"Рейтинг KP: {filmKinopoiskUnofficalModel.RatingKinopoisk}, Рейтинг IMDB: {filmKinopoiskUnofficalModel.RatingImdb}",
                                chatId, cancellationToken, inlineKeyboard);
                        }
                        
                        await context.SetMessage(user.Id, command);
                    }
                }
                else
                {
                    await context.SetMessage(user.Id, command);
                    await client.SendDefaultMessage(
                        "Ничего не найдено, попробуйте уточнить запрос.",
                        chatId, cancellationToken);
                }
            }
        }
    }
}
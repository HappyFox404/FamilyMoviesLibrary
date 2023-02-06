using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Context.Models;
using FamilyMoviesLibrary.Helpers;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Models;
using FamilyMoviesLibrary.Models.Atributes;
using FamilyMoviesLibrary.Models.Data;
using FamilyMoviesLibrary.Services;
using FamilyMoviesLibrary.Services.Helpers;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using User = Telegram.Bot.Types.User;

namespace FamilyMoviesLibrary.BotCommands;

[BotCommand]
public class RecommendFilmCommand : IBotCommand
{
    public bool IsNeedCommand(string command)
    {
        return new CommandBuilder(command).DefinationCommand(BotCommandNames.RecommendFilm);
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

            var needUser = await context.Users.Include(x => x.Group)
                .FirstOrDefaultAsync(x => x.TelegramId == user.Id);
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
                if (StorageService.Genres.Any())
                {
                    await client.SendDefaultMessage(
                        $"Подскажите фильмы какой категории вы хотите посмотреть? Доступные: " +
                        $"{String.Join(", ", StorageService.Genres.Select(x => x.Genre).ToList())}",
                        chatId, cancellationToken);
                    await context.SetMessage(user.Id, command, true);
                }
                else
                {
                    await client.SendDefaultMessage(
                        "Нет данных о досутпных жанрах. Обратитесь в техподдержку.",
                        chatId, cancellationToken);
                    await context.SetMessage(user.Id, command);
                }
            }
            else
            {
                string continueArgument = buildCommand.GetContinueValue();
                if (StorageService.Genres.Any())
                {
                    var needGenre = StorageService.Genres
                        .FirstOrDefault(x => x.Genre.ToLower().Contains(continueArgument.ToLower()));
                    if (needGenre != default)
                    {
                        //Здесь нужно добавить, чтобы он брал только просмотренные фильмы определённого жанра.
                        List<long> viewedFilms = await context.Films.Where(x => x.GroupId == needUser.GroupId)
                            .Select(x => x.KinopoiskId).ToListAsync();
                        int maxPageSearch = 20;
                        List<int> viewedPages = new List<int>();
                        Random rnd = new Random();
                        int page = GetRandom(rnd, 0, maxPageSearch, viewedPages);
                        while (viewedPages.Count() <= maxPageSearch)
                        {
                            List<FilmKinopoiskUnofficalModel> requestedFilm = 
                                await new ApiKinopoiskRequestHelper().RecommendFilm(needGenre.Id,page);
                            if (requestedFilm.Any())
                            {
                                var notViewedFilms = requestedFilm
                                    .Where(x => viewedFilms.Contains(x.KinopoiskId) == false).ToList();
                                if (notViewedFilms.Any())
                                {
                                    var recommendIndex = GetRandom(rnd, 0, notViewedFilms.Count(), new List<int>());
                                    var recommendFilm = notViewedFilms.ElementAtOrDefault(recommendIndex);
                                    if (recommendFilm != default)
                                    {
                                        InlineKeyboardMarkup inlineKeyboard = new(new[]
                                        {
                                            new[]
                                            {
                                                InlineKeyboardButton.WithCallbackData(text: "Оценить и добавить в просмотренное",
                                                    callbackData: $"{BotCommandNames.LikeFilm} -i:{recommendFilm.KinopoiskId}")
                                            }
                                        });
                                        string countries = String.Join(" ",
                                            recommendFilm.Countries.Where(x => x.Country != default)
                                                .Select(x => x.Country).ToList());
                                        string genres = String.Join(" ",
                                            recommendFilm.Genres.Where(x => x.Genre != default)
                                                .Select(x => x.Genre).ToList());
                                        await client.SendDefaultMessage(
                                            $"{recommendFilm.PosterUrl}\n" +
                                            $"{recommendFilm.NameRu} {((recommendFilm.Year != default) ? recommendFilm : "Не указан")}\n" +
                                            $"Страны: {countries}.\n" +
                                            $"Жанры: {genres}\n" +
                                            $"Рейтинг KP: {recommendFilm.RatingKinopoisk}, Рейтинг IMDB: {recommendFilm.RatingImdb}",
                                            chatId, cancellationToken, inlineKeyboard);
                                        await context.SetMessage(user.Id, command);
                                        return;
                                    }
                                    else
                                    {
                                        await client.SendDefaultMessage(
                                            $"Произошла непредвиденная ошибка, обратитесь в тех. поддержку.",
                                            chatId, cancellationToken);
                                        await context.SetMessage(user.Id, command);
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                if (page > 0)
                                {
                                    maxPageSearch = page - 1;
                                }
                                else
                                {
                                    viewedPages.Add(page);
                                }
                            }
                            page = GetRandom(rnd, 0, maxPageSearch, viewedPages);
                        }
                        await client.SendDefaultMessage(
                            $"К удивлению Вы просмотрели все фильмы жанра: {continueArgument}, которые я смог найти на Кинопоиск мне нечего Вам посоветовать :D",
                            chatId, cancellationToken);
                        await context.SetMessage(user.Id, command);
                    }
                    else
                    {
                        await client.SendDefaultMessage(
                            $"Не смог определить жанр, вы ввели {continueArgument}, попробуйте снова.",
                            chatId, cancellationToken);
                        await context.SetMessage(user.Id, command);
                    }
                }
                else
                {
                    await client.SendDefaultMessage(
                        "Нет данных о досутпных жанрах. Обратитесь в техподдержку.",
                        chatId, cancellationToken);
                    await context.SetMessage(user.Id, command);
                }
            }
        }
    }
    
    private int GetRandom(Random rand, int min, int max, List<int> exclude)
    {
        int result = 0;
        do
        {
            result = rand.Next(min, max+1);
        } while (exclude.Contains(result));

        return result;
    }
}
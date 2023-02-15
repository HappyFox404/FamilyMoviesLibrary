using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Context.ContextQuery;
using FamilyMoviesLibrary.Context.Models;
using FamilyMoviesLibrary.Models;
using FamilyMoviesLibrary.Models.Atributes;
using FamilyMoviesLibrary.Models.Data;
using FamilyMoviesLibrary.Models.Exception;
using FamilyMoviesLibrary.Models.Extension;
using FamilyMoviesLibrary.Services;
using FamilyMoviesLibrary.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

    public async Task ExecuteCommand(FamilyMoviesLibraryContext context, string command, TelegramBotClient client, Update update, IServiceProvider collection,
        CancellationToken cancellationToken)
    {
        var buildCommand = new CommandBuilder(command);
        if (buildCommand.ValidCommand)
        {
            IStorageService? storage = collection.GetService<IStorageService>();
            IFilmService? filmService = collection.GetService<IFilmService>();
            if (storage == null)
                throw new ControllException("Не удалось получить сервис (Storage)", false);
            if (filmService == null)
                throw new ControllException("Не удалось получить сервис (Films)", false);
            
            User user = update.GetUser();
            ChatId chatId = update.GetChatId();

            var needUser = await context.GetUser(user.Id);

            if (needUser.Group == default)
            {
                await client.SendDefaultMessage(
                    "Вы не находитесь в библиотеке! Вступите в библиотеку или создайте новую.",
                    chatId, cancellationToken);
                return;
            }
            
            if (buildCommand.ContainsContinueKey() == false)
            {
                await client.SendDefaultMessage(
                    $"Укажите фильм какой категории вы хотите посмотреть? Доступные: " +
                    $"{String.Join(", ", storage.GetGenres().Select(x => x.Genre).ToList())}",
                    chatId, cancellationToken);
                await context.SetMessage(user.Id, command, true);
            }
            else
            {
                string continueArgument = buildCommand.GetContinueValue();
                
                var needGenre = storage.GetGenres()
                    .FirstOrDefault(x => x.Genre.ToLower().Contains(continueArgument.ToLower()));
                if (needGenre != default)
                {
                    int maxPageSearch = 20;
                    List<int> viewedPages = new List<int>();
                    Random rnd = new Random();
                    int page = GetRandom(rnd, 0, maxPageSearch, viewedPages);
                    
                    while (viewedPages.Count() <= maxPageSearch)
                    {
                        IEnumerable<FilmKinopoiskUnofficalModel> requestedFilm = await filmService.RecommendFilm(needGenre.Id, page);
                        
                        if (requestedFilm.Any())
                        {
                            List<long> viewedFilms = await context.Films.Where(x => x.GroupId == needUser.GroupId && x.GenreId == needGenre.Id)
                                .Select(x => x.KinopoiskId).ToListAsync();
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
                                        $"{recommendFilm.NameRu} ({((recommendFilm.Year != default) ? recommendFilm.Year : "Год не указан")})\n" +
                                        $"Страны: {countries}.\n" +
                                        $"Жанры: {genres}\n" +
                                        $"Рейтинг KP: {recommendFilm.RatingKinopoisk}, Рейтинг IMDB: {recommendFilm.RatingImdb}",
                                        chatId, cancellationToken, inlineKeyboard);
                                    await context.SetMessage(user.Id, command);
                                    return;
                                }
                                
                                throw new ControllException(
                                    "Произошла непредвиденная ошибка, обратитесь в тех. поддержку.");
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
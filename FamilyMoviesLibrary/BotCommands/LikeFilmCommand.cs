using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Context.Models;
using FamilyMoviesLibrary.Helpers;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Models;
using FamilyMoviesLibrary.Models.Atributes;
using FamilyMoviesLibrary.Models.Data;
using FamilyMoviesLibrary.Services.Helpers;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using User = Telegram.Bot.Types.User;

namespace FamilyMoviesLibrary.BotCommands;

[BotCommand]
public class LikeFilmCommand : IBotCommand
{
    public bool IsNeedCommand(string command)
    {
        return new CommandBuilder(command).DefinationCommand(BotCommandNames.LikeFilm);
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
                long kpId = 0;
                try
                {
                    kpId = Convert.ToInt64(buildCommand.GetArgumentValue("i"));
                }
                catch
                {
                    await client.SendDefaultMessage(
                        "Произошла системная ошибка. Не найден id кинопоиск.",
                        chatId, cancellationToken);
                    await context.SetMessage(user.Id, command);
                    return;
                }
                var searchFilm = await context.Films.FirstOrDefaultAsync(x =>
                    x.GroupId == needUser.Group.Id && x.KinopoiskId == kpId);
                if (searchFilm == default)
                {
                    await context.SetMessage(user.Id, command, true);
                    await client.SendDefaultMessage(
                        "Укажите рейтинг фильма от 0 до 10, включительно:",
                        chatId, cancellationToken);

                    await context.SetMessage(user.Id, command, true);
                }
                else
                {
                    await client.SendDefaultMessage(
                        $"Данный фильм уже есть в вашей бибилотеке и имеет оценку: {searchFilm.Rate}",
                        chatId, cancellationToken);
                    await context.SetMessage(user.Id, command);
                }
            }
            else
            {
                string continueArgument = buildCommand.GetContinueValue();
                int userRate = 0;
                try
                {
                    userRate = Convert.ToInt32(continueArgument);
                }
                catch
                {
                    await client.SendDefaultMessage(
                        "Произошла ошибка при проставлении рейтинга. (Возможно вы ввели слово вместо цифр. Допустимые значения от 0 до 10)",
                        chatId, cancellationToken);
                    await context.SetMessage(user.Id, command);
                    return;
                }

                if (userRate < 0 || userRate > 10)
                {
                    await client.SendDefaultMessage(
                        "Допустимый диапозон значений от 0 до 10...",
                        chatId, cancellationToken);
                    await context.SetMessage(user.Id, command);
                    return;
                }

                long kpId = 0;
                try
                {
                    kpId = Convert.ToInt64(buildCommand.GetArgumentValue("i"));
                }
                catch
                {
                    await client.SendDefaultMessage(
                        "Произошла системная ошибка. Не найден id кинопоиск.",
                        chatId, cancellationToken);
                    await context.SetMessage(user.Id, command);
                    return;
                }

                try
                {
                    var searchFilm = await context.Films.FirstOrDefaultAsync(x =>
                        x.GroupId == needUser.Group.Id && x.KinopoiskId == kpId);
                    if (searchFilm == default)
                    {
                        context.Films.Add(new Film()
                        {
                            Id = Guid.NewGuid(),
                            KinopoiskId = kpId,
                            Rate = userRate,
                            GroupId = needUser.Group.Id
                        });
                        await context.SaveChangesAsync();
                        await client.SendDefaultMessage(
                            "Ваша оценка добавлена к фильму и занесена в вашу библиотеку.",
                            chatId, cancellationToken);
                        await context.SetMessage(user.Id, command);
                    }
                    else
                    {
                        await client.SendDefaultMessage(
                            $"Данный фильм уже есть в вашей бибилотеке и имеет оценку: {searchFilm.Rate}",
                            chatId, cancellationToken);
                        await context.SetMessage(user.Id, command);
                    }
                }
                catch
                {
                    await client.SendDefaultMessage(
                        "произошла системная ошибка. Не удалось добавить фильм в библиотеку.",
                        chatId, cancellationToken);
                    await context.SetMessage(user.Id, command);
                }

            }
        }
    }
}
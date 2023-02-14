using FamilyMoviesLibrary.Context.Models;
using FamilyMoviesLibrary.Models.Exception;
using Microsoft.EntityFrameworkCore;

namespace FamilyMoviesLibrary.Context.ContextQuery;

public static partial class FamilyMoviesLibraryExtension
{
    /// <summary>
    /// Создание нового пользователя, если он есть ничего не происходит
    /// </summary>
    /// <param name="context">контекст</param>
    /// <param name="telegramUser">id пользователя в телеграм</param>
    public static async Task CreateUser(this FamilyMoviesLibraryContext context, Telegram.Bot.Types.User telegramUser)
    {
        var needUser = await context.Users.FirstOrDefaultAsync(x => x.TelegramId == telegramUser.Id);
        if (needUser == default)
        {
            context.Users.Add(new User() { Id = Guid.NewGuid(), TelegramId = telegramUser.Id});
            await context.SaveChangesAsync();
        }
        else
        {
            if (String.IsNullOrWhiteSpace(needUser.TelegramUserName))
            {
                if (String.IsNullOrWhiteSpace(telegramUser.Username) == false)
                    needUser.TelegramUserName = telegramUser.Username;
                else
                    needUser.TelegramUserName = $"{telegramUser.FirstName} {telegramUser.LastName}";
                await context.SaveChangesAsync();
            }
        }
    }

    /// <summary>
    /// Получение пользователя
    /// </summary>
    /// <param name="context">контекст</param>
    /// <param name="userId">id пользователя в телеграм</param>
    /// <returns></returns>
    /// <exception cref="ControllException">Пользователь не найден</exception>
    public static async Task<User> GetUser(this FamilyMoviesLibraryContext context, long userId)
    {
        var needUser = await context.Users
            .Include(x => x.Group)
            .Include(x => x.Message)
            .FirstOrDefaultAsync(x => x.TelegramId == userId);
        if (needUser == default)
        {
            throw new ControllException($"Не найден пользователь в БД. {userId}", false);
        }
        return needUser;
    }
}
using FamilyMoviesLibrary.Context.Models;
using FamilyMoviesLibrary.Models.Exception;
using Microsoft.EntityFrameworkCore;

namespace FamilyMoviesLibrary.Context.ContextQuery;

public static partial class FamilyMoviesLibraryExtension
{
    /// <summary>
    /// Удаление сообщения пользователя
    /// </summary>
    /// <param name="context">контекст</param>
    /// <param name="userId">id пользователя telegram</param>
    public static void ClearMessage(this FamilyMoviesLibraryContext context, long userId)
    {
        if (context.Users.Any(x => x.TelegramId == userId))
        {
            var user = context.Users.Include(x => x.Message)
                .FirstOrDefault(x => x.TelegramId == userId);
            if (user != default && user.Message != default)
            {
                context.Messages.Remove(user.Message);
            }

            context.SaveChanges();
        }
    }
    
    /// <summary>
    /// Сохранение сообщения
    /// </summary>
    /// <param name="context">контекст</param>
    /// <param name="userId">id пользователя telegram</param>
    /// <param name="message">текст сообщения</param>
    /// <param name="isNeedAdditionalMessage">Требует продолжения обработки?</param>
    public static async Task SetMessage(this FamilyMoviesLibraryContext context, long userId, string? message, bool isNeedAdditionalMessage = false)
    {
        if (context.Users.Any(x => x.TelegramId == userId))
        {
            var user = await context.Users.Include(x => x.Message)
                .FirstOrDefaultAsync(x => x.TelegramId == userId);
            if (user != default)
            {
                if (user.Message != default)
                    context.ClearMessage(userId);

                await context.Messages.AddAsync(new Message()
                {
                    Id = Guid.NewGuid(),
                    Data = message,
                    IsNeedAdditionalMessage = isNeedAdditionalMessage,
                    UserId = user.Id
                });

                await context.SaveChangesAsync();
            }
        }
    }
    
    /// <summary>
    /// Требует ли сообщения пользователя обработки?
    /// </summary>
    /// <param name="context">контекст</param>
    /// <param name="userId">id пользователя telegram</param>
    /// <returns></returns>
    public static async Task<bool> ContinueLastMessage(this FamilyMoviesLibraryContext context, long userId)
    {
        if (context.Users.Any(x => x.TelegramId == userId))
        {
            var user = await context.Users.Include(x => x.Message)
                .FirstOrDefaultAsync(x => x.TelegramId == userId);
            if (user?.Message != null)
            {
                return user.Message.IsNeedAdditionalMessage;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Получить текст сообщения
    /// </summary>
    /// <param name="context">контекст</param>
    /// <param name="userId">id пользователя telegram</param>
    /// <returns></returns>
    /// <exception cref="ControllException">Нет сообщения или сообщение равно NULL</exception>
    public static async Task<string> LastMessage(this FamilyMoviesLibraryContext context, long userId)
    {
        if (context.Users.Any(x => x.TelegramId == userId))
        {
            var user = await context.Users.Include(x => x.Message)
                .FirstOrDefaultAsync(x => x.TelegramId == userId);
            if (user?.Message != null)
            {
                return user?.Message?.Data ?? 
                       throw new ControllException($"Не удалось получить данные о последнем сообщение: {userId}", false);
            }
        }
        throw new ControllException($"Не удалось получить данные о последнем сообщение: {userId}", false);
    }
}
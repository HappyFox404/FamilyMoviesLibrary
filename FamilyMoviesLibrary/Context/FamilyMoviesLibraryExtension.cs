using FamilyMoviesLibrary.Context.Models;
using FamilyMoviesLibrary.Models.Exception;
using FamilyMoviesLibrary.Services;
using Microsoft.EntityFrameworkCore;

namespace FamilyMoviesLibrary.Context;

public static class FamilyMoviesLibraryExtension
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
    
    /// <summary>
    /// Создание группы
    /// </summary>
    /// <param name="context">контекст</param>
    /// <param name="userId">id пользователя telegram</param>
    /// <param name="groupName">имя группы</param>
    /// <exception cref="ControllException">Причина, по которой не выполнено</exception>
    public static async Task CreateGroup(this FamilyMoviesLibraryContext context, long userId, string groupName)
    {
        var user = await context.GetUser(userId);
        if (user?.Group == default)
        {
            if (context.Groups.Any(x => x.Name.ToLower() == groupName.ToLower()) == false)
            {
                var newGroupId = Guid.NewGuid();
                await context.Groups.AddAsync(new Group()
                {
                    Id = newGroupId,
                    Name = groupName
                });
                await context.SaveChangesAsync();
                user.GroupId = newGroupId;
                await context.SaveChangesAsync();
                return;
            }
            throw new ControllException("Данная библиотека существует.");
        }
        throw new ControllException("Пользователь уже находиться в бибилотеке, для создания выйдите из текущей.");
    }
    
    /// <summary>
    /// Выход пользователя из группы
    /// </summary>
    /// <param name="context">контекст</param>
    /// <param name="userId">id пользователя telegram</param>
    /// <exception cref="ControllException">Причина, по которой не выполнено</exception>
    public static async Task ExitGroup(this FamilyMoviesLibraryContext context, long userId)
    {
        var user = await context.GetUser(userId);
        if (user.Group != default)
        {
            Guid? groupId = user?.Group.Id;
            if (groupId.HasValue)
            {
                user.GroupId = null;
                await context.SaveChangesAsync();
                var needGroup = await context.Groups.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == groupId);
                if (needGroup != default && needGroup.Users.Any() == false)
                {
                    context.DeleteGroup(groupId.Value);
                }
                return;
            }
        }
        throw new ControllException("Библиотека не найдена.");
    }
    
    /// <summary>
    /// Удаление группы
    /// </summary>
    /// <param name="context">контекст</param>
    /// <param name="userId">id пользователя telegram</param>
    /// <exception cref="ControllException">Причина, по которой не выполнено</exception>
    public static void DeleteGroup(this FamilyMoviesLibraryContext context, Guid groupId)
    {
        if (context.Groups.Any(x => x.Id == groupId))
        {
            var needGroup = context.Groups.Include(x => x.Users).FirstOrDefault(x => x.Id == groupId);
            if (needGroup != null && needGroup.Users.Any() == false)
            {
                context.Groups.Remove(needGroup);
                context.SaveChanges();
                return;
            }
        }
        throw new ControllException($"Не найдена библиотека {groupId}", false);
    }
}
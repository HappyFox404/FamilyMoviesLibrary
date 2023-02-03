using FamilyMoviesLibrary.Context.Models;
using FamilyMoviesLibrary.Models.Exception;
using FamilyMoviesLibrary.Services;
using Microsoft.EntityFrameworkCore;

namespace FamilyMoviesLibrary.Context;

public static class FamilyMoviesLibraryExtension
{
    public static async Task<bool> CreateUser(this FamilyMoviesLibraryContext context, Telegram.Bot.Types.User telegramUser)
    {
        var needUser = await context.Users.FirstOrDefaultAsync(x => x.TelegramId == telegramUser.Id);
        if (needUser == default)
        {
            context.Users.Add(new User() { Id = Guid.NewGuid(), TelegramId = telegramUser.Id});
            int changes = await context.SaveChangesAsync();

            if (changes > 0)
                return true;
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

        return false;
    }
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
    public static async Task<string?> LastMessage(this FamilyMoviesLibraryContext context, long userId)
    {
        if (context.Users.Any(x => x.TelegramId == userId))
        {
            var user = await context.Users.Include(x => x.Message)
                .FirstOrDefaultAsync(x => x.TelegramId == userId);
            if (user?.Message != null)
            {
                return user.Message.Data;
            }
        }
        return null;
    }
    public static async Task CreateGroup(this FamilyMoviesLibraryContext context, long userId, string groupName)
    {
        if (context.Users.Any(x => x.TelegramId == userId))
        {
            var user = await context.Users.Include(x => x.Group)
                .FirstOrDefaultAsync(x => x.TelegramId == userId);
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
                }
                else
                {
                    throw new ControllException("Группа с таким именем уже существует! Придумайте другое название.");
                }
            }
            else
            {
                throw new ControllException("Пользователь уже находиться в группе, для создания выйдите из текущей.");
            }
        }
        else
        {
            throw new ControllException("Не найден пользователь!");
        }
    }

    public static async Task ExitGroup(this FamilyMoviesLibraryContext context, long userId)
    {
        if (context.Users.Any(x => x.TelegramId == userId))
        {
            var user = await context.Users.Include(x => x.Group)
                .FirstOrDefaultAsync(x => x.TelegramId == userId);
            if (user?.Group != default)
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
            else
            {
                throw new ControllException("Вы не находитесь в группе!");
            }
        }
        else
        {
            throw new ControllException("Не найден пользователь!");
        }
        throw new ControllException("Произошла непредвиденная ошибка!");
    }
    
    public static void DeleteGroup(this FamilyMoviesLibraryContext context, Guid groupId)
    {
        if (context.Groups.Any(x => x.Id == groupId))
        {
            var needGroup = context.Groups.Include(x => x.Users).FirstOrDefault(x => x.Id == groupId);
            if (needGroup != null && needGroup.Users.Any() == false)
            {
                context.Groups.Remove(needGroup);
                context.SaveChanges();
            }
        }
        else
        {
            throw new ControllException("Не найдена группа!");
        }
    }
}
using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Context.Models;
using FamilyMoviesLibrary.Models.Exception;
using FamilyMoviesLibrary.Services;
using Microsoft.EntityFrameworkCore;

namespace FamilyMoviesLibrary.Helpers;

public class DatabaseHelper
{
    public static async Task<bool> CreateUser(long userId)
    {
        using (FamilyMoviesLibraryContext context =
               FamilyMoviesLibraryContext.CreateContext(SettingsService.GetDefaultConnectionString()))
        {
            if (context.Users.Any(x => x.TelegramId == userId) == false)
            {
                context.Users.Add(new User() { Id = Guid.NewGuid(), TelegramId = userId});
                int changes = await context.SaveChangesAsync();

                if (changes > 0)
                    return true;
            }

            return false;
        }
    }
    public static async Task ClearMessage(long userId)
    {
        using (FamilyMoviesLibraryContext context =
               FamilyMoviesLibraryContext.CreateContext(SettingsService.GetDefaultConnectionString()))
        {
            if (context.Users.Any(x => x.TelegramId == userId))
            {
                var user = await context.Users.Include(x => x.Message)
                    .FirstOrDefaultAsync(x => x.TelegramId == userId);
                if (user != default && user.Message != default)
                {
                    context.Messages.Remove(user.Message);
                }

                await context.SaveChangesAsync();
            }
        }
    }
    public static async Task SetMessage(long userId, string? message, bool isNeedAdditionalMessage = false)
    {
        using (FamilyMoviesLibraryContext context =
               FamilyMoviesLibraryContext.CreateContext(SettingsService.GetDefaultConnectionString()))
        {
            if (context.Users.Any(x => x.TelegramId == userId))
            {
                var user = await context.Users.Include(x => x.Message)
                    .FirstOrDefaultAsync(x => x.TelegramId == userId);
                if (user != default)
                {
                    if (user.Message != default)
                        await DatabaseHelper.ClearMessage(userId);

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
    }
    public static async Task<bool> ContinueLastMessage(long userId)
    {
        using (FamilyMoviesLibraryContext context =
               FamilyMoviesLibraryContext.CreateContext(SettingsService.GetDefaultConnectionString()))
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
        }
        return false;
    }
    public static async Task<string?> LastMessage(long userId)
    {
        using (FamilyMoviesLibraryContext context =
               FamilyMoviesLibraryContext.CreateContext(SettingsService.GetDefaultConnectionString()))
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
        }
        return null;
    }
    public static async Task CreateGroup(long userId, string groupName)
    {
        using (FamilyMoviesLibraryContext context =
               FamilyMoviesLibraryContext.CreateContext(SettingsService.GetDefaultConnectionString()))
        {
            if (context.Users.Any(x => x.TelegramId == userId))
            {
                var user = await context.Users.Include(x => x.Group)
                    .FirstOrDefaultAsync(x => x.TelegramId == userId);
                if (user?.Group == default)
                {
                    if (context.Groups.Any(x => x.Name == groupName) == false)
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
    }
}
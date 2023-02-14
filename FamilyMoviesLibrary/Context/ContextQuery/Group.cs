using FamilyMoviesLibrary.Context.Models;
using FamilyMoviesLibrary.Models.Exception;
using Microsoft.EntityFrameworkCore;

namespace FamilyMoviesLibrary.Context.ContextQuery;

public static partial class FamilyMoviesLibraryExtension
{
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
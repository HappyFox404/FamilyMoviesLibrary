using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Context.Models;
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
}
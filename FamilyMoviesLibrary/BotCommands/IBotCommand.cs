using FamilyMoviesLibrary.Context;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FamilyMoviesLibrary.BotCommands;

public interface IBotCommand
{
    bool IsNeedCommand(string command);
    Task ExecuteCommand(FamilyMoviesLibraryContext context, string command, TelegramBotClient client, 
        Update update, IServiceProvider collection, CancellationToken cancellationToken);
}
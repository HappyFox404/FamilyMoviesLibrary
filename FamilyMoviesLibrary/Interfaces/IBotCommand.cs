using Telegram.Bot;
using Telegram.Bot.Types;

namespace FamilyMoviesLibrary.Interfaces;

public interface IBotCommand
{
    bool IsNeedCommand(string command);
    Task ExecuteCommand(string command, TelegramBotClient client, Update update, CancellationToken cancellationToken);
}
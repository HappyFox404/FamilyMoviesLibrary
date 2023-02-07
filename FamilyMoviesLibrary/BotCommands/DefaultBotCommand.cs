using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Helpers;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Models.Atributes;
using FamilyMoviesLibrary.Models.Exception;
using FamilyMoviesLibrary.Services.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyMoviesLibrary.BotCommands;

[BotCommand(true)]
public class DefaultBotCommand : IBotCommand
{
    public bool IsNeedCommand(string command)
    {
        return true;
    }

    public async Task ExecuteCommand(FamilyMoviesLibraryContext context, string command, TelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        if (update.Message != default || update.CallbackQuery != default)
        {
            User user = TelegramHelper.GetUser(update);
            ChatId chatId = TelegramHelper.GetChatId(update);
            
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                new [] { InlineKeyboardButton.WithCallbackData(text: "Помощь", callbackData: "/help") }
            });

            await context.SetMessage(user.Id, command);
            await client.SendDefaultMessage(
                "Я не понимаю, что вы хотите. Список доступных комманд вы можете узнать при помощи /help",
                chatId, cancellationToken, inlineKeyboard);
        }
    }
}
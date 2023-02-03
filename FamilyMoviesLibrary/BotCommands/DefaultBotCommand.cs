using FamilyMoviesLibrary.Helpers;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Services.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyMoviesLibrary.BotCommands;

public class DefaultBotCommand : IBotCommand
{
    public bool IsNeedCommand(string command)
    {
        return true;
    }

    public async Task ExecuteCommand(string command, TelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        if (update.Message != default || update.CallbackQuery != default)
        {
            User? user = TelegramHelper.GetUser(update);
            if (user == default)
                return;
            
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                new [] { InlineKeyboardButton.WithCallbackData(text: "Помощь", callbackData: "/help") }
            });
            
            ChatId? chatId = TelegramHelper.GetChatId(update);

            await client.SendDefaultMessage(
                "Я не понимаю, что вы хотите. Список доступных комманд вы можете узнать при помощи /help",
                chatId, cancellationToken, inlineKeyboard);
            await DatabaseHelper.SetMessage(user.Id, command);
        }
    }
}
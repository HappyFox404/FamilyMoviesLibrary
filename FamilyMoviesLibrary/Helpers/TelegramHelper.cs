using System.Diagnostics;
using Telegram.Bot.Types;

namespace FamilyMoviesLibrary.Services.Helpers;

public class TelegramHelper
{
    public static ChatId? GetChatId(Update update)
    {
        ChatId? chatId = null;
        if (update.Message != default)
            chatId = update.Message.Chat.Id;
        else if (update.CallbackQuery != default && update.CallbackQuery.Message != default)
            chatId = update.CallbackQuery.Message.Chat.Id;
        return chatId;
    }
    
    public static User? GetUser(Update update)
    {
        User? user = null;
        if (update.Message != default)
            user = update.Message.From;
        else if (update.CallbackQuery != default && update.CallbackQuery.Message != default)
            user = update.CallbackQuery.Message.From;
        return user;
    }
}
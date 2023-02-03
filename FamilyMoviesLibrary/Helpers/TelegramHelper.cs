using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

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
        else if (update.CallbackQuery != default)
            user = update.CallbackQuery.From;
        return user;
    }
}

public static class TelegramBotClientExtension {
    public static async Task<Message?> SendDefaultMessage(this TelegramBotClient client, string message, ChatId? chatId, 
        CancellationToken cancellationToken, InlineKeyboardMarkup? inlineKeyboard = null)
    {
        if (chatId != default)
        {
            if (inlineKeyboard == default)
            {
                return await client.SendTextMessageAsync(
                    chatId: chatId,
                    text: message,
                    replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken);
            }
            return await client.SendTextMessageAsync(
                chatId: chatId,
                text: message,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
        return default;
    }
}
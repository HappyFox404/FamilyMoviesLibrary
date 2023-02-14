using FamilyMoviesLibrary.Models.Exception;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyMoviesLibrary.Models.Extension;
public static class TelegramBotClientExtension {
    /// <summary>
    /// Получить ChatId из update
    /// </summary>
    /// <param name="update">telegram update</param>
    /// <returns></returns>
    /// <exception cref="ControllException">Почему не обработано</exception>
    public static ChatId GetChatId(this Update update)
    {
        if (update.Message != default)
            return update.Message.Chat.Id;
        if (update.CallbackQuery != default && update.CallbackQuery.Message != default)
            return update.CallbackQuery.Message.Chat.Id;
        
        throw new ControllException($"Не смог определить чат с пользователем телеграм.", false);
    }
    
    /// <summary>
    /// Получить Telegram User из update
    /// </summary>
    /// <param name="update">telegram update</param>
    /// <returns></returns>
    /// <exception cref="ControllException">Почему не обработано</exception>
    public static User GetUser(this Update update)
    {
        if (update.Message != default && update.Message.From != default)
            return update.Message.From;
        if (update.CallbackQuery != default)
            return update.CallbackQuery.From;
        
        throw new ControllException($"Не смог определить пользователя телеграм.", false);
    }
    /// <summary>
    /// Отправка стандартного сообщения от бота
    /// </summary>
    /// <param name="client">telegram client</param>
    /// <param name="message">сообщение</param>
    /// <param name="chatId">telegram ChatId</param>
    /// <param name="cancellationToken"></param>
    /// <param name="inlineKeyboard">Кнопки обратного вызова</param>
    /// <returns></returns>
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
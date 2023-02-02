using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Services.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyMoviesLibrary.BotCommands;

public class HelpBotCommand : IBotCommand
{
    public bool IsNeedCommand(string command)
    {
        var buildCommand = new CommandBuilder(command);
        if (buildCommand.ValidCommand && 
            buildCommand.Command == "/help")
        {
            return true;
        }
        return false;
    }

    public async Task ExecuteCommand(string command, TelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        if (update.Message != default || update.CallbackQuery != default)
        {
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "Помощь", callbackData: "/help"),
                    InlineKeyboardButton.WithCallbackData(text: "Группа", callbackData: "/group"),
                }
            });

            ChatId chatId = TelegramHelper.GetChatId(update);

            if (chatId != default)
            {
                Message sendMessage = await client.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Список доступных команд:",
                    replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken);
            }
        }
    }
}
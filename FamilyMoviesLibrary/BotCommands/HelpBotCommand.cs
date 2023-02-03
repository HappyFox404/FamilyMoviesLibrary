﻿using FamilyMoviesLibrary.Helpers;
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
            User? user = TelegramHelper.GetUser(update);
            if (user == default)
                return;
            
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "Помощь", callbackData: "/help"),
                    InlineKeyboardButton.WithCallbackData(text: "Группа", callbackData: "/group"),
                }
            });

            ChatId chatId = TelegramHelper.GetChatId(update);
            
            await client.SendDefaultMessage("Список доступных команд:",
                chatId, cancellationToken, inlineKeyboard);
            await DatabaseHelper.SetMessage(user.Id, command);
        }
    }
}
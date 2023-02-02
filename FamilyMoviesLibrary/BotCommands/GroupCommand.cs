﻿using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Services.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyMoviesLibrary.BotCommands;

public class GroupCommand : IBotCommand
{
    public bool IsNeedCommand(string command)
    {
        var buildCommand = new CommandBuilder(command);
        if (buildCommand.ValidCommand && 
            buildCommand.Command == "/group")
        {
            return true;
        }
        return false;
    }

    public async Task ExecuteCommand(string command, TelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        var buildCommand = new CommandBuilder(command);
        if (buildCommand.ValidCommand)
        {
            if (buildCommand.Arguments.Any() == false)
            {
                await GenerateBaseResponse(client, update, cancellationToken);
            }
        }
    }
    
    private async Task GenerateBaseResponse(TelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Помощь", callbackData: "/help"),
                InlineKeyboardButton.WithCallbackData(text: "Группа", callbackData: "/group"),
            }
        });
        
        ChatId chatId;
        if (update.Message != default)
            chatId = update.Message.Chat.Id;
        else if (update.CallbackQuery != default && update.CallbackQuery.Message != default)
            chatId = update.CallbackQuery.Message.Chat.Id;
        else
            throw new ArgumentException("Не получилось получить ChatId");
            
        Message sendMessage = await client.SendTextMessageAsync(
            chatId: chatId,
            text: "Список доступных команд:",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }
}
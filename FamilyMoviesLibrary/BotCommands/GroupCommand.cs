﻿using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Helpers;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Models;
using FamilyMoviesLibrary.Models.Atributes;
using FamilyMoviesLibrary.Services;
using FamilyMoviesLibrary.Services.Helpers;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyMoviesLibrary.BotCommands;

[BotCommand]
public class GroupCommand : IBotCommand
{
    public bool IsNeedCommand(string command)
    {
        return new CommandBuilder(command).DefinationCommand(BotCommandNames.Group);
    }

    public async Task ExecuteCommand(FamilyMoviesLibraryContext context, string command, TelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        var buildCommand = new CommandBuilder(command);
        if (buildCommand.ValidCommand)
        {
            ChatId? chatId = TelegramHelper.GetChatId(update);
            User? user = TelegramHelper.GetUser(update);
            
            if (user == default)
                return;

            var userData = context.Users.Include(x => x.Group).FirstOrDefault(x => x.TelegramId == user.Id);
            InlineKeyboardMarkup inlineKeyboard;
            if (userData?.Group != default)
            {
                inlineKeyboard = new(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Выйти из группы",
                            callbackData: BotCommandNames.GroupExit),
                        InlineKeyboardButton.WithCallbackData(text: "Участники",
                            callbackData: $"{BotCommandNames.GroupUsers} -g:{userData.Group.Id}")
                    }
                });
            }
            else
            {
                inlineKeyboard = new(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Создать группу",
                            callbackData: BotCommandNames.GroupCreate),
                        InlineKeyboardButton.WithCallbackData(text: "Найти группу",
                            callbackData: BotCommandNames.GroupSearch),
                    }
                });
            }
            await context.SetMessage(user.Id, command);
            await client.SendDefaultMessage(
                "Вот что я могу Вам предложить:",
                chatId, cancellationToken, inlineKeyboard);
        }
    }
}
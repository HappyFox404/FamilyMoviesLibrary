using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Models;
using FamilyMoviesLibrary.Models.Atributes;
using FamilyMoviesLibrary.Services.Helpers;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyMoviesLibrary.BotCommands;

[BotCommand]
public class GroupSearchCommand : IBotCommand
{
    public bool IsNeedCommand(string command)
    {
        return new CommandBuilder(command).DefinationCommand(BotCommandNames.GroupSearch);
    }

    public async Task ExecuteCommand(FamilyMoviesLibraryContext context, string command, TelegramBotClient client, Update update,
        CancellationToken cancellationToken)
    {
        var buildCommand = new CommandBuilder(command);
        if (buildCommand.ValidCommand)
        {
            ChatId? chatId = TelegramHelper.GetChatId(update);
            User? user = TelegramHelper.GetUser(update);
            
            if (user == default)
                return;
            
            if (buildCommand.ContainsContinueKey() == false)
            {
                await context.SetMessage(user.Id, command, true);
                await client.SendDefaultMessage(
                    "Введите полное название или часть, искомой библиотеки:",
                    chatId, cancellationToken);
            }
            else
            {
                string continueArgument = buildCommand.GetContinueValue();
                List<string> similarityGroups = await context.Groups
                    .Where(x => x.Name.ToLower().Contains(continueArgument.ToLower()) == true)
                    .Select(x => x.Name).ToListAsync();
                
                await context.SetMessage(user.Id, command);
                InlineKeyboardMarkup inlineKeyboard = null;
                if (similarityGroups.Any())
                {
                    await client.SendDefaultMessage(
                        "Вот что я нашёл по вашему запросу:",
                        chatId, cancellationToken, inlineKeyboard);
                    
                    foreach (var similarityGroup in similarityGroups)
                    {
                        inlineKeyboard = new(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData(text: "Вступить в библиотеку",
                                    callbackData: $"{BotCommandNames.GroupConnect} \"-c:{similarityGroup}\"")
                            }
                        });
                        await client.SendDefaultMessage(
                            $"Билиотека: {similarityGroup}",
                            chatId, cancellationToken, inlineKeyboard);
                    }
                }
                /*await client.SendDefaultMessage(
                    messageResponse,
                    chatId, cancellationToken, inlineKeyboard);*/
            }
        }
    }
}
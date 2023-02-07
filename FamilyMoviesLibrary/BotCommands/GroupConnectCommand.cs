using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Models;
using FamilyMoviesLibrary.Models.Atributes;
using FamilyMoviesLibrary.Models.Exception;
using FamilyMoviesLibrary.Services.Helpers;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyMoviesLibrary.BotCommands;

[BotCommand]
public class GroupConnectCommand : IBotCommand
{
    public bool IsNeedCommand(string command)
    {
        return new CommandBuilder(command).DefinationCommand(BotCommandNames.GroupConnect);
    }

    public async Task ExecuteCommand(FamilyMoviesLibraryContext context, string command, TelegramBotClient client, Update update,
        CancellationToken cancellationToken)
    {
        var buildCommand = new CommandBuilder(command);
        if (buildCommand.ValidCommand)
        {
            ChatId chatId = TelegramHelper.GetChatId(update);
            User user = TelegramHelper.GetUser(update);
            
            if (buildCommand.ContainsContinueKey() == false)
            {
                await context.SetMessage(user.Id, command, true);
                await client.SendDefaultMessage(
                    "Введите название библиотеки для присоединения:",
                    chatId, cancellationToken);
            }
            else
            {
                string continueArgument = buildCommand.GetContinueValue();

                var needGroup = await context.Groups
                    .Where(x => x.Name.ToLower().Contains(continueArgument.ToLower()) == true)
                    .ToListAsync();
                
                InlineKeyboardMarkup inlineKeyboard = null;
                string messageResponse = "Не найдено ни одной библиотеки.";
                if (needGroup.Any())
                {
                    if (needGroup.Count() == 1)
                    {
                        var needUser = await context.Users.FirstOrDefaultAsync(x => x.TelegramId == user.Id);
                        if (needUser != default)
                        {
                            needUser.GroupId = needGroup.FirstOrDefault()?.Id;
                            await context.SaveChangesAsync();
                            messageResponse = "Вы успешно присоединились к библиотеке!";
                        }
                    }
                    else
                    {
                        messageResponse = "При поиске бибилотеки выдало более одного совпадения, повторите попытку. (Возможно вы опечатались)";
                        inlineKeyboard = new(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData(text: "Вступить в библиотеку",
                                    callbackData: BotCommandNames.GroupConnect)
                            }
                        });
                    }
                }
                
                await context.SetMessage(user.Id, command);
                await client.SendDefaultMessage(
                    messageResponse,
                    chatId, cancellationToken, inlineKeyboard);
            }
        }
    }
}
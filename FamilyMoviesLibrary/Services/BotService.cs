using FamilyMoviesLibrary.ApplicationCommands;
using FamilyMoviesLibrary.BotCommands;
using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Helpers;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Models.Exception;
using FamilyMoviesLibrary.Services.Helpers;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FamilyMoviesLibrary.Services;

public class BotService
{
    private readonly TelegramBotClient _client;
    public delegate void WaitAction();
    private readonly WaitAction _waitingAction;

    private readonly IEnumerable<IBotCommand> _commands;

    private readonly IBotCommand _defaultCommand;
    
    public BotService(string token, WaitAction waitingAction)
    {
        _commands = SystemHelper.GetBotCommands();
        _defaultCommand = SystemHelper.GetBotDefaultCommand();
        _waitingAction = waitingAction;
        _client = new TelegramBotClient(token);
    }

    public async Task StartBot()
    {
        using CancellationTokenSource cts = new ();
        
        _client.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: new ()
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            },
            cancellationToken: cts.Token);

        await _client.GetMeAsync();
        
        _waitingAction?.Invoke();
        
        cts.Cancel();
    }
    
    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message
            || update.Type == UpdateType.CallbackQuery)
        {
            
            
            if (update.Message?.Text != default)
                await ListenCommands(update.Message?.Text, update, cancellationToken);
            else if(update.CallbackQuery?.Data != default)
                await ListenCommands(update.CallbackQuery?.Data, update, cancellationToken);
        }
    }

    Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
    
    private async Task ListenCommands(string sendCommand, Update update, CancellationToken cancellationToken)
    {
        string resultCommand = sendCommand;
        //Кароче нужна обработка предыдущих сообщений
        using (FamilyMoviesLibraryContext context =
               FamilyMoviesLibraryContext.CreateContext(SettingsService.GetDefaultConnectionString()))
        {
            try
            {
                User telegramUser = TelegramHelper.GetUser(update);
                await context.CreateUser(telegramUser);

                if (String.IsNullOrWhiteSpace(resultCommand) == false)
                {
                    if (await context.ContinueLastMessage(telegramUser.Id))
                    {
                        string prevCommand = await context.LastMessage(telegramUser.Id);
                        if (String.IsNullOrWhiteSpace(prevCommand) == false)
                        {
                            resultCommand = $"{prevCommand} \"{CommandBuilder.ContinueKey}{sendCommand}\"";
                        }
                    }

                    bool foundCommand = false;
                    foreach (var command in _commands)
                    {
                        if (command.IsNeedCommand(resultCommand))
                        {
                            foundCommand = true;
                            await command.ExecuteCommand(context, resultCommand, _client, update, cancellationToken);
                            break;
                        }
                    }

                    if (!foundCommand)
                    {
                        await _defaultCommand.ExecuteCommand(context, resultCommand, _client, update,
                            cancellationToken);
                    }
                }
                else
                {
                    await _defaultCommand?.ExecuteCommand(context, "", _client, update, cancellationToken)!;
                }
            }
            catch (ControllException controll)
            {
                if (controll.UserAnswer)
                {
                    User? user = TelegramHelper.GetUser(update);
                    ChatId? chatId = TelegramHelper.GetChatId(update);
                    await context.SetMessage(user.Id, "error");
                    await _client.SendDefaultMessage(
                        controll.Message,
                        chatId, cancellationToken);
                }
                else
                {
                    Console.WriteLine($"Во время обработки запроса, произошла контролируемая ошибка: {controll.Message}");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Во время обработки запроса, произошла ошибка: {exception}");
            }
        }
    }
}
using FamilyMoviesLibrary.ApplicationCommands;
using FamilyMoviesLibrary.BotCommands;
using FamilyMoviesLibrary.Helpers;
using FamilyMoviesLibrary.Interfaces;
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
    
    private readonly IEnumerable<IBotCommand> _commands = new List<IBotCommand>()
    {
        new HelpBotCommand(), new GroupCommand(), new GroupCreateCommand()
    };

    private readonly IBotCommand _defaultCommand = new DefaultBotCommand();
    
    public BotService(string token, WaitAction waitingAction)
    {
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
        try
        {
            User? telegramUser = TelegramHelper.GetUser(update);
            if (telegramUser != default)
            {
                await DatabaseHelper.CreateUser(telegramUser.Id);
            }
            if (String.IsNullOrWhiteSpace(resultCommand) == false)
            {
                if (telegramUser != null && await DatabaseHelper.ContinueLastMessage(telegramUser.Id))
                {
                    string? prevCommand = await DatabaseHelper.LastMessage(telegramUser.Id);
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
                        await command.ExecuteCommand(resultCommand, _client, update, cancellationToken);
                        break;
                    }
                }
                if (!foundCommand)
                {
                    await _defaultCommand.ExecuteCommand(resultCommand, _client, update, cancellationToken);
                }
            }
            else
            {
                await _defaultCommand?.ExecuteCommand("", _client, update, cancellationToken)!;
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Во время обработки запроса, произошла ошибка: {exception}");
        }
    }
}
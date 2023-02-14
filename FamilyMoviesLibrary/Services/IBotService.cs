using FamilyMoviesLibrary.BotCommands;
using FamilyMoviesLibrary.Context;
using FamilyMoviesLibrary.Helpers;
using FamilyMoviesLibrary.Interfaces;
using FamilyMoviesLibrary.Models.Exception;
using FamilyMoviesLibrary.Models.Extension;
using FamilyMoviesLibrary.Models.Settings;
using FamilyMoviesLibrary.Services.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FamilyMoviesLibrary.Services;

public interface IBotService
{
    Task StartBot();
}

public class BotService : IBotService
{
    private readonly TelegramBotClient _client;
    private readonly IEnumerable<IBotCommand> _commands;
    private readonly IBotCommand _defaultCommand;
    private readonly ILogger<BotService> _logger;
    private readonly FamilyMoviesLibraryContext _context;
    private readonly IStorageService _storage;

    public BotService(ILogger<BotService> logger, IOptions<TelegramSettings> telegramSettings, 
        FamilyMoviesLibraryContext context, IStorageService storage)
    {
        _logger = logger;
        _context = context;
        _storage = storage;
        _commands = SystemHelper.GetBotCommands();
        _defaultCommand = SystemHelper.GetBotDefaultCommand();
        _client = new TelegramBotClient(telegramSettings.Value.Token);
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
        _logger.LogInformation("Bot is started!");
        Console.ReadKey();
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
        try
            {
                User telegramUser = update.GetUser();
                await _context.CreateUser(telegramUser);

                if (String.IsNullOrWhiteSpace(resultCommand) == false)
                {
                    if (await _context.ContinueLastMessage(telegramUser.Id))
                    {
                        string prevCommand = await _context.LastMessage(telegramUser.Id);
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
                            await command.ExecuteCommand(_context, resultCommand, _client, update, cancellationToken);
                            break;
                        }
                    }

                    if (!foundCommand)
                    {
                        await _defaultCommand.ExecuteCommand(_context, resultCommand, _client, update,
                            cancellationToken);
                    }
                }
                else
                {
                    await _defaultCommand?.ExecuteCommand(_context, "", _client, update, cancellationToken)!;
                }
            }
            catch (ControllException controll)
            {
                if (controll.UserAnswer)
                {
                    User user = update.GetUser();
                    ChatId chatId = update.GetChatId();
                    await _context.SetMessage(user.Id, "error");
                    await _client.SendDefaultMessage(
                        controll.Message,
                        chatId, cancellationToken);
                }
                else
                {
                    _logger.LogError($"Во время обработки запроса, произошла контролируемая ошибка: {controll.Message}");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Во время обработки запроса, произошла ошибка: {exception}");
            }
    }
}
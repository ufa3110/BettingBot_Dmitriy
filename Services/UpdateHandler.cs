using BettingBot.Database;
using BettingBot.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace BettingBot.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly BotContext _botContext;

    public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger, BotContext botContext)
    {
        _botClient = botClient;
        _logger = logger;
        _botContext = botContext;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            { Message: { } message }                       => BotOnMessageReceived(message, cancellationToken),
            { EditedMessage: { } message }                 => BotOnMessageReceived(message, cancellationToken),
        };

        await handler;
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        if (!MessageValidationHelper.ValidateIputMessage(message))
        {
            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Ошибка сохранения. Неверный формат.",
                cancellationToken: cancellationToken);
            return;
        }
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);

        var bet = InputMessageParser.ParseInputMessage(message.Text);

        var result = await SaveBet(bet, message.Chat.Id);

        if (result)
        {
            _logger.LogDebug("Успешное сохранение записи в файл и бд.");
        }
    }

    private async Task<bool> SaveBet(Bet bet, long chatId)
    {
        try
        {
            _botContext.Bets.Add(bet);
            _botContext.BKs.AddRange(bet.Bks);
            _botContext.SaveChanges();
            SyncTable();
            return true;
        }
        catch (IOException ex)
        {
            await _botClient.SendTextMessageAsync(
               chatId: chatId,
               text: "Файл в настоящее время открыт, изменения в файле появятся после следующего сообщения.");
            return false;
        }
        catch (Exception ex) 
        {
            await _botClient.SendTextMessageAsync(
               chatId: chatId,
               text: $"Неизвестная ошибка: {ex.ToString()}");
            return false;
        }
    }

    private void SyncTable()
    {
        var bets = 
            _botContext.Bets
            .Include(b => b.Bks)
            .ToList();
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("БК1;БК2;Спорт;Исход;Коэф;Команды");

        foreach(var bet in bets)
        {
            stringBuilder.AppendLine($"{bet.Bks[0].Name};{bet.Bks[1].Name};{bet.Sport};{bet.Bks[0].Title};{bet.Bks[0].Coefficient};{bet.Title}");
        }
        System.IO.File.WriteAllText("Output.csv", stringBuilder.ToString(), Encoding.UTF8);
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}

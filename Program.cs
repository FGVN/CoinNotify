using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using CoinNotify.Models;

var botClient = new TelegramBotClient("6333881612:AAEB9jLJR4jVj8HldNL9jdhU4SoM6qwY1qc");

//getting coins
CoinParser parser = new CoinParser();
var coins = parser.GetCoins();

using CancellationTokenSource cts = new();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
        return;
    // Only process text messages
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;

    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");


    if(message.Text == "Check")
    {
        List<KeyboardButton[]> buttonData = new List<KeyboardButton[]>();

        foreach (var i in coins)
        {
            buttonData.Add( new KeyboardButton[] { i._name } );
        }


        ReplyKeyboardMarkup inlineKeyboard = new ReplyKeyboardMarkup(buttonData.ToArray());

        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "A message with an inline keyboard markup",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);

        
    }
    if (coins.Select(x => x._name).Contains(update.Message.Text))
    {
        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: coins.FirstOrDefault(x => x._name == update.Message.Text)._price.ToString(),
            cancellationToken: cancellationToken);
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
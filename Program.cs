using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using CoinNotify.Models;
using CoinNotify.Controllers;
using System.Collections.Generic;
using System.Diagnostics;

var botClient = new TelegramBotClient("6333881612:AAEB9jLJR4jVj8HldNL9jdhU4SoM6qwY1qc");

//getting coins
CoinParser parser = new CoinParser();
var coins = parser.GetCoins();

UsersController usersController = new UsersController();

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



    if (message.Text == "View Current Prices")
    {
        List<KeyboardButton[]> buttonData = new List<KeyboardButton[]>();

        foreach (var i in coins)
        {
            buttonData.Add(new KeyboardButton[] { i._name });
        }


        ReplyKeyboardMarkup inlineKeyboard = new ReplyKeyboardMarkup(buttonData.ToArray());

        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Choose a coin",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);


    }
    else if (coins.Select(x => x._name).Contains(update.Message.Text))
    {
        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: coins.FirstOrDefault(x => x._name == update.Message.Text)._price.ToString(),
            cancellationToken: cancellationToken);
    }
    else if (message.Text == "Set new notification")
    {
        List<KeyboardButton[]> buttonData = new List<KeyboardButton[]>();

        foreach (var i in coins)
        {
            buttonData.Add(new KeyboardButton[] { i._name + " - " + i._price });
        }


        ReplyKeyboardMarkup inlineKeyboard = new ReplyKeyboardMarkup(buttonData.ToArray());

        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Choose coin",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }
    else if (coins.Select(x => x._name).Contains(update.Message.Text.ToString().Split(" - ")[0])
        && update.Message.Text.ToString().Split(" - ")[1] != null)
    {
        Console.WriteLine("coin set choose");
        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Enter a price for notification in format: \n'coin_name : notification_price'",
            cancellationToken: cancellationToken);
    }
    else if (coins.Select(x => x._name).Contains(update.Message.Text.ToString().Split(" : ")[0])
        && update.Message.Text.ToString().Split(" : ")[1] != null) 
    {
        var coin_name = update.Message.Text.ToString().Split(" : ")[0];
        var coin_price = update.Message.Text.ToString().Split(" : ")[1];

        bool res = usersController.AddNotify(chatId.ToString(), coin_name, coin_price);

        if (res)
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Notification added",
            cancellationToken: cancellationToken);
        }
        else
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Error occured",
            cancellationToken: cancellationToken);
        }
    }
    else if (message.Text == "Check my notifications")
    {
        var notifications = usersController.GetNotifications(chatId.ToString());
        if (notifications.Any())
        {
            foreach(var i in notifications)
            {
                Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: i._name + " - " + i._price,
                cancellationToken: cancellationToken);
            }
        }
        else
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Currently you dont have any notifications setted",
            cancellationToken: cancellationToken);
        }
    }
    else if(message.Text == "Delete notifiaction")
    {
        var notifications = usersController.GetNotifications(chatId.ToString());
        if (notifications.Any())
        {
            foreach (var i in notifications)
            {
                Message sentMessageMany = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: i._name + " - " + i._price,
                cancellationToken: cancellationToken);
            }
            Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Enter notification to delete in format:\ndelete 'coin_name'",
            cancellationToken: cancellationToken);
        }
        else
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Currently you dont have any notifications setted",
            cancellationToken: cancellationToken);
        }

    }

    else if (message.Text.ToString().Split(" ")[0].Contains("delete"))
    {
        string coin_name = message.Text.ToString().Split(" ")[1];
        usersController.DeleteNotification(chatId.ToString(), coin_name);
        //add incorrect data handling

        Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "Notification deleted",
        cancellationToken: cancellationToken);
    }
    else
    {

        List<KeyboardButton[]> buttonData = new List<KeyboardButton[]>();

        buttonData.Add(new KeyboardButton[] { "Check my notifications" });
        buttonData.Add(new KeyboardButton[] { "Set new notification" });
        buttonData.Add(new KeyboardButton[] { "Delete notifiaction" });
        buttonData.Add(new KeyboardButton[] { "View Current Prices" });

        //"Check my notifications", "Set new notification", "Delete notifiaction", "View Current Prices" }
        ReplyKeyboardMarkup inlineKeyboard = new ReplyKeyboardMarkup(buttonData.ToArray());

        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "What do you want to do?",
            replyMarkup: inlineKeyboard,
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
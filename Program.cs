using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using CoinNotify.Models;
using CoinNotify.Controllers;
using System.Diagnostics;

string key;

Console.WriteLine("Enter telegram bot key: ");
key = Console.ReadLine();

var botClient = new TelegramBotClient(key);

//getting coins
CoinParser parser = new CoinParser();
var coins = CoinParser.GetCoins();

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

while (true)
{
    UpdateInfo(cts.Token);

    await Task.Delay(TimeSpan.FromSeconds(20));
}

Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();


//Updates data about coins and handles notifications 
async Task UpdateInfo(CancellationToken cancellationToken)
{
    Console.WriteLine("Check");

    //Updating coins values
    coins = CoinParser.GetCoins();

    NotificationController notificationController = new NotificationController();
        foreach( var i in 
            notificationController.CheckNotifications(usersController.GetUsers(), coins))
        {
        //Sending message for every notification price
            Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: i._id,
            text: "Coin " + i._coins[0]._name + " has reached its notify price: " + Math.Abs(i._coins[0]._price),
            cancellationToken: cancellationToken);

        //Deleting notifications that has been triggered
        usersController.DeleteNotification(
                i._id, 
                (usersController.GetUsers().
                First(x => x._id == i._id)._coins.IndexOf(
                    new Coin(
                        i._coins[0]._name,
                        usersController.GetUsers().First(x => x._id == i._id)
                        ._coins.First(x => x._name == i._coins[0]._name)._price
                    )
                )+2).ToString()
           );

        }

    // Wait for 60 seconds before calling the method again
    await Task.Delay(TimeSpan.FromSeconds(60));
}

//Handles messages from users
async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
        return;
    // Only process text messages
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;

    //A bit of spaghetti code beacuse c# switch cases don`t handle Linq statements :(
    //But here is all of the message handling logic
    if (message.Text == "View Current Prices")
    {
        PriceViewHandle(message, chatId, cancellationToken);
    }
    else if (coins.Select(x => x._name).Contains(update.Message.Text))
    {
        PriceHandle(message, chatId, cancellationToken);
    }
    else if (message.Text == "Set new notification")
    {
        NotificationHandle(message, chatId, cancellationToken);
    }
    else if (coins.Select(x => x._name).Contains(update.Message.Text.ToString().Split(" - ")[0])
        && update.Message.Text.ToString().Split(" - ")[1] != null)
    {
        NotificationSelectHandle(message, chatId, cancellationToken);
    }
    else if ((coins.Select(x => x._name).Contains(update.Message.Text.ToString().Split(" < ")[0])
        && update.Message.Text.ToString().Split(" < ")[1] != null) ||
        (coins.Select(x => x._name).Contains(update.Message.Text.ToString().Split(" > ")[0])
        && update.Message.Text.ToString().Split(" > ")[1] != null)) 
    {
        NotificationAddHandle(message, chatId, cancellationToken);
    }
    else if (message.Text == "Check my notifications")
    {
        CheckHandle(message, chatId, cancellationToken);
    }
    else if(message.Text == "Delete notifiaction")
    {
        DeleteNotificationHandle(message, chatId, cancellationToken);
    }
    else if (message.Text.ToString().Split(" ")[0].Contains("delete"))
    {
        DeleteHandle(message, chatId, cancellationToken);
    }
    else
    {
        DefaultHandle(chatId, cancellationToken);
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

//Returns buttons with all trackable coins name
async void PriceViewHandle(Message message, long chatId, CancellationToken cancellationToken)
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

//Returns price of the coin
async void PriceHandle(Message message, long chatId, CancellationToken cancellationToken)
{
    Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: coins.FirstOrDefault(x => x._name == message.Text)._price.ToString(),
            cancellationToken: cancellationToken);

    //Sending default keyboard
    DefaultHandle(chatId, cancellationToken);
}

//Returns buttons with all trackable coins and their prices
async void NotificationHandle(Message message, long chatId, CancellationToken cancellationToken)
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

//After some button is pressed in previous method - asks for sending message in certain form
async void NotificationSelectHandle(Message message, long chatId, CancellationToken cancellationToken)
{
    Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "Enter a price for notification in format: \n'coin_name > or < notification_price'",
        cancellationToken: cancellationToken);
}

//Reaceives message with notification setup
async void NotificationAddHandle(Message message, long chatId, CancellationToken cancellationToken)
{
    string coin_name, coin_price;
    //if tracking price below some value - saving as coin with negative price
    if (coins.Select(x => x._name).Contains(message.Text.ToString().Split(" < ")[0])
    && message.Text.ToString().Split(" < ")[1] != null)
    {
        coin_name = message.Text.ToString().Split(" < ")[0];
        coin_price = "-" + message.Text.ToString().Split(" < ")[1];
    }
    //if tracking price higher some value - as usual
    else
    {
        coin_name = message.Text.ToString().Split(" > ")[0];
        coin_price = message.Text.ToString().Split(" > ")[1];
    }

    
    //Passing data into the controller and responding according to the result
    bool res = usersController.AddNotify(chatId.ToString(), coin_name, coin_price);

    string reply;

    if (res)
        reply = "Notification added";
    else
        reply = "Error occured";

    Message sentMessage = await botClient.SendTextMessageAsync(
    chatId: chatId,
    text: reply,
    cancellationToken: cancellationToken);
}

//Returns all notifications that some user has set up
async void CheckHandle(Message message, long chatId, CancellationToken cancellationToken)
{
    var notifications = usersController.GetNotifications(chatId.ToString());
    if (notifications.Any())
    {
        foreach (var i in notifications)
        {
            string reply;
            if (i._price < 0)
                reply = i._name + " < " + Math.Abs(i._price);
            else
                reply = i._name + " > " + i._price;

            Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: reply,
            cancellationToken: cancellationToken);
        }
    }
    else
    {
        Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "Currently you dont have any notifications setted",
        cancellationToken: cancellationToken);

        //Sending default keyboard
        DefaultHandle(chatId, cancellationToken);
    }
}


//Returns all users notifications and asks to send delete message in certain format
async void DeleteNotificationHandle(Message message, long chatId, CancellationToken cancellationToken)
{
    var notifications = usersController.GetNotifications(chatId.ToString());
    if (notifications.Any())
    {
        int count = 1;
        string reply;
        foreach (var i in notifications)
        {
            if (i._price < 0)
                reply = count + " " + i._name + " < " + Math.Abs(i._price);
            else
                reply = count + " " + i._name + " > " + i._price;

            Message sentMessageMany = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: reply,
            cancellationToken: cancellationToken);
            count++;
        }
        Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "Enter notification to delete in format:\ndelete 'index_of_notification'",
        cancellationToken: cancellationToken);
    }
    else
    {
        Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "Currently you dont have any notifications setted",
        cancellationToken: cancellationToken);

        //Sending default keyboard
        DefaultHandle(chatId, cancellationToken);
    }
}

//Sends notification to delete value to controller
async void DeleteHandle(Message message, long chatId, CancellationToken cancellationToken)
{
    string notify_index = message.Text.ToString().Split(" ")[1];
    usersController.DeleteNotification(chatId.ToString(), notify_index);

    Message sentMessage = await botClient.SendTextMessageAsync(
    chatId: chatId,
    text: "Notification deleted",
    cancellationToken: cancellationToken);

    //Sending default keyboard
    DefaultHandle(chatId, cancellationToken);
}

//Main menu that returns default keyboard
async void DefaultHandle(long chatId, CancellationToken cancellationToken)
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
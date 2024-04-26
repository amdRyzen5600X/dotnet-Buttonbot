using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

var botClient = new TelegramBotClient("BOT_TOKEN");

using CancellationTokenSource cts = new ();

ReceiverOptions receiverOptions = new () {
    AllowedUpdates = Array.Empty<UpdateType>()
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine("bot started");

Console.ReadLine();

cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken){
    if (update.Message is not { } message)
        return;
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;

    var splitedText = messageText.Split("\n");
    var textButtons = splitedText.Last();
    var text = splitedText.Take(splitedText.Length - 1);

    List<InlineKeyboardButton> buttons = new ();
    foreach (var button in textButtons.Split("]")){
        var buttonText = button.Split(" [").First();
        var buttonUrl = button.Split(" [").Last();
        buttons.Add(InlineKeyboardButton.WithUrl(
            text: buttonText,
            url: buttonUrl
        ));
    }

    InlineKeyboardButton[] arrayButtons = buttons.ToArray();

    InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(
        arrayButtons.Take(arrayButtons.Length - 1).ToArray().Chunk(3).ToArray()
    );

    if ((buttons is not null) && (text is not null)){
        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: String.Join("\n", text),
            cancellationToken: cancellationToken,
            replyMarkup: inlineKeyboardMarkup
        );
    }
    
}

async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken){
    var ErrorMessage = exception switch {
        ApiRequestException apiRequestException 
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _=> exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    await Task.CompletedTask;
}


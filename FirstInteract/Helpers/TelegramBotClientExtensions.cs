using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FirstInteract.Helpers;

public static class TelegramBotClientExtensions
{
    public static async Task<Message> SendMessageWithDefaultButtons(this ITelegramBotClient botClient, Chat chat,
        string text, CancellationToken cancellationToken = default)
    {
        // Создаем клавиатуру с зашитыми кнопками
        var replyMarkup = new ReplyKeyboardMarkup([["/show"], ["/addtask", "/report"]])
        {
            ResizeKeyboard = true
        };


        return await botClient.SendMessage(
            chatId: chat.Id,
            text: text,
            replyMarkup: replyMarkup,
            cancellationToken: cancellationToken);
    }

    public static async Task<Message> SendMessageWithCancelButton(this ITelegramBotClient botClient, Chat chat,
        string text, CancellationToken cancellationToken = default)
    {
        // Создаем клавиатуру с зашитыми кнопками
        var replyMarkup = new ReplyKeyboardMarkup([["/cancel"]])
        {
            ResizeKeyboard = true
        };

        return await botClient.SendMessage(
            chatId: chat.Id,
            text: text,
            replyMarkup: replyMarkup,
            cancellationToken: cancellationToken);
    }
}
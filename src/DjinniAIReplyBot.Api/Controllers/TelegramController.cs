using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DjinniAIReplyBot.Api.Controllers;

[ApiController]
[Route("api/telegram")]
public class TelegramController : ControllerBase
{
    private TelegramBotClient _bot = Bot.GetTelegramBot();
    
    private static ConcurrentDictionary<long, CancellationTokenSource> _runningTasks = new ConcurrentDictionary<long, CancellationTokenSource>();
    
    [HttpPost]
public async Task<IActionResult> Post([FromBody] Update update)
{
       
    try
    {
        var chatId = update.Message.Chat.Id;
        var messageText = update.Message.Text;
        
        if (messageText == "/start")
        {
            if (!_runningTasks.ContainsKey(chatId))
            {
                var cts = new CancellationTokenSource();

                // Додаємо його до словника
                _runningTasks[chatId] = cts;

                _ = SendMessagesEveryInterval(chatId, "ХАХАХАХ", 50, cts.Token);

                
                await _bot.SendTextMessageAsync(chatId, "Починаю надсилати повідомлення.");
            }
            else
            {
                await _bot.SendTextMessageAsync(chatId, "Відправка повідомлень вже запущена.");
            }
        }
        else
        {
            // Перевіряємо, чи є запущений таск для цього чату
            if (_runningTasks.TryRemove(chatId, out var cts))
            {
                // Скасовуємо таск
                cts.Cancel();

                await _bot.SendTextMessageAsync(chatId, "Зупиняю надсилання повідомлень.");
            }
            else
            {
                await _bot.SendTextMessageAsync(chatId, "Я не розумію тебе. Напиши /start, щоб почати спілкування.");
            }
        }

        return Ok();
    }
    catch (Exception e)
    {
        if (_runningTasks.TryRemove(update.MyChatMember.Chat.Id, out var cts))
            cts.Cancel();
        Console.WriteLine(e.Message);
        return BadRequest(e.Message);
    }
    
}

private async Task SendMessagesEveryInterval(long chatId, string message, int intervalMilliseconds, CancellationToken cancellationToken)
{
    try
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await _bot.SendTextMessageAsync(chatId, message, cancellationToken: cancellationToken);
            await Task.Delay(intervalMilliseconds, cancellationToken);
        }
    }
    catch (OperationCanceledException)
    {
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error sending message to chat {chatId}: {ex.Message}");
    }
}
    
    [HttpGet]
    public string Get() 
    {
        return "Telegram bot was started";
    }
}

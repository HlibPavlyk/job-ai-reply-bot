using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using DjinniAIReplyBot.Application.Services;
using DjinniAIReplyBot.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace DjinniAIReplyBot.Api.Controllers;

[ApiController]
[Route("api/telegram")]
public class TelegramController : ControllerBase
{
    private readonly CommandExecutor _updateDistributor;
    private readonly ILogger<TelegramController> _logger;
    private readonly ITelegramService _client;

    public TelegramController(ILogger<TelegramController> logger, CommandExecutor updateDistributor, ITelegramService client)
    {
        _logger = logger;
        _updateDistributor = updateDistributor;
        _client = client;
    }
    
    [HttpPost]
    public async Task<IActionResult> Post(Update update)
    {

        try
        {
            await _updateDistributor.GetUpdate(update);
            return Ok("Update was processed");
        }
        catch (UserNotificationException e)
        {
            await _client.SendMessageAsync(e.ChatId, e.Message);
            _logger.LogWarning(e.Message, "User notification exception");
            return Ok("User notification exception" + e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while processing update");
            return BadRequest("Error while processing update: " + e.Message);
        }

    }

    [HttpGet]
    public string Get()
    {
        return "Telegram bot was started";
    }
}
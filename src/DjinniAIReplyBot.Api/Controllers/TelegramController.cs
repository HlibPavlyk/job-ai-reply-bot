using DjinniAIReplyBot.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace DjinniAIReplyBot.Api.Controllers;

[ApiController]
[Route("api/telegram")]
public class TelegramController : ControllerBase
{
    private readonly UpdateDistributor<CommandExecutor> _updateDistributor;
    private readonly ILogger<TelegramController> _logger;

    public TelegramController(UpdateDistributor<CommandExecutor> updateDistributor, ILogger<TelegramController> logger)
    {
        _updateDistributor = updateDistributor;
        _logger = logger;
    }
    
    [HttpPost]
    public async Task<IActionResult> Post(Update update)
    {
        if (update.Message == null)
        {
            _logger.LogWarning("Update message is null");
            return BadRequest("Update message is null");
        }

        try
        {
            await _updateDistributor.GetUpdate(update);
            return Ok("Update was processed");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while processing update");
            return BadRequest("Error while processing update");
        }

    }

    [HttpGet]
    public string Get()
    {
        return "Telegram bot was started";
    }
}
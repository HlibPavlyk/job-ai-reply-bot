using DjinniAIReplyBot.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace DjinniAIReplyBot.Api.Controllers;

[ApiController]
[Route("api/telegram")]
public class TelegramController : ControllerBase
{
    private readonly CommandExecutor _updateDistributor;
    private readonly ILogger<TelegramController> _logger;

    public TelegramController(ILogger<TelegramController> logger, CommandExecutor updateDistributor)
    {
        _logger = logger;
        _updateDistributor = updateDistributor;
    }
    
    [HttpPost]
    public async Task<IActionResult> Post(Update update)
    {
       
        try
        {
            await _updateDistributor.GetUpdate(update);
            return Ok("Update was processed");
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
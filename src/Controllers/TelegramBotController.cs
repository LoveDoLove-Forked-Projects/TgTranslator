﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using TgTranslator.Services;
using TgTranslator.Utils.Extensions;

namespace TgTranslator.Controllers;

[Route("api/bot")]
[ServiceFilter(typeof(IpWhitelist))]
public class TelegramBotController : Controller
{
    private readonly HandlersRouter _handlersRouter;

    public TelegramBotController(HandlersRouter handlersRouter)
    {
        _handlersRouter = handlersRouter;
    }

    [HttpGet]
    public IActionResult Get() => Ok();

    [HttpPost]
    public async Task<OkResult> Post()
    {
        var body = await HttpContext.Request.GetRawBodyAsync();
        if (body == null)
            return Ok();

        var update = JsonConvert.DeserializeObject<Update>(body);
        await _handlersRouter.HandleUpdate(update);

        return Ok();
    }
}

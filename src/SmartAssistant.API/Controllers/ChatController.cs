using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartAssistant.Application.Chat.Commands.AskQuestion;

namespace SmartAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> AskQuestion([FromBody] AskQuestionCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
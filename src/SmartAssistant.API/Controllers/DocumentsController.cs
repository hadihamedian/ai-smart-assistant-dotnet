using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartAssistant.Application.Documents.Commands.UploadDocument;

namespace SmartAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("File is empty");

        using var stream = file.OpenReadStream();
        var command = new UploadDocumentCommand(file.FileName, stream);
        var documentId = await _mediator.Send(command);

        return Ok(new { DocumentId = documentId });
    }
}
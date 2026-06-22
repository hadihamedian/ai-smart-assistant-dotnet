using MediatR;

namespace SmartAssistant.Application.Documents.Commands.UploadDocument;

public record UploadDocumentCommand(string FileName, Stream FileStream) : IRequest<Guid>;
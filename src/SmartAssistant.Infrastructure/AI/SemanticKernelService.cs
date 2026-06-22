using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SmartAssistant.Application.Common.Interfaces;

namespace SmartAssistant.Infrastructure.AI;

public class SemanticKernelService : IAIService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletion;

    public SemanticKernelService(Kernel kernel)
    {
        _kernel = kernel;
        _chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<string> GenerateAnswerAsync(string question, IEnumerable<string> contextChunks, CancellationToken cancellationToken = default)
    {
        var contextText = new StringBuilder();
        foreach (var chunk in contextChunks)
        {
            contextText.AppendLine(chunk);
        }

        var prompt = $@"You are a helpful assistant. Use the following context to answer the question.
Context:
{contextText}

Question: {question}";

        var history = new ChatHistory("You are a smart document assistant.");
        history.AddUserMessage(prompt);

        var response = await _chatCompletion.GetChatMessageContentAsync(history, cancellationToken: cancellationToken);
        return response?.Content ?? "No answer generated.";
    }
}
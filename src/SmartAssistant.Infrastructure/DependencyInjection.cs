using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Qdrant.Client;
using SmartAssistant.Application.Common.Interfaces;
using SmartAssistant.Domain.Interfaces;
using SmartAssistant.Infrastructure.AI;
using SmartAssistant.Infrastructure.Parsers;
using SmartAssistant.Infrastructure.Repositories;
using SmartAssistant.Infrastructure.VectorStore;

namespace SmartAssistant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDocumentRepository, DocumentRepository>();
        
        var qdrantHost = configuration["Qdrant:Host"] ?? "localhost";
        var qdrantPort = int.Parse(configuration["Qdrant:Port"] ?? "6334");
        services.AddSingleton(new QdrantClient(qdrantHost, qdrantPort));

        services.AddScoped<IVectorStore, QdrantVectorStore>();
        // ارجاع صریح به اینترفیس برنامه برای جلوگیری از تداخل
        services.AddScoped<SmartAssistant.Application.Common.Interfaces.IAIService, SemanticKernelService>();

        var provider = configuration["AI:Provider"];
        var builder = Kernel.CreateBuilder();

        if (provider == "OpenAI")
        {
            builder.AddOpenAIChatCompletion(
                configuration["AI:OpenAI:ChatModel"]!,
                configuration["AI:OpenAI:ApiKey"]!);

#pragma warning disable SKEXP0010
            builder.AddOpenAIEmbeddingGenerator(
                configuration["AI:OpenAI:EmbeddingModel"]!,
                configuration["AI:OpenAI:ApiKey"]!);
#pragma warning restore SKEXP0010
        }
        else
        {
            var ollamaUrl = configuration["AI:Ollama:BaseUrl"]!.TrimEnd('/') + "/v1/";
            var customHttpClient = new HttpClient { BaseAddress = new Uri(ollamaUrl) };

            builder.AddOpenAIChatCompletion(
                configuration["AI:Ollama:ChatModel"]!,
                apiKey: "ollama",
                httpClient: customHttpClient);

#pragma warning disable SKEXP0010
            builder.AddOpenAIEmbeddingGenerator(
                configuration["AI:Ollama:EmbeddingModel"]!,
                apiKey: "ollama",
                httpClient: customHttpClient);
#pragma warning restore SKEXP0010
        }

        var kernel = builder.Build();
        services.AddSingleton(kernel);
        
        // استخراج و ثبت صریح EmbeddingGenerator از کرنل به DI دات‌نت
        services.AddSingleton(sp => kernel.GetRequiredService<Microsoft.Extensions.AI.IEmbeddingGenerator<string, Microsoft.Extensions.AI.Embedding<float>>>());

        services.AddSingleton<IChatSessionRepository, ChatSessionRepository>();
        services.AddSingleton<IDocumentParser, PdfDocumentParser>();
        
        return services;
    }
}

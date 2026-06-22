using Qdrant.Client;
using Qdrant.Client.Grpc;
using SmartAssistant.Domain.Interfaces;
using Microsoft.Extensions.AI;

namespace SmartAssistant.Infrastructure.VectorStore;

public class QdrantVectorStore : IVectorStore
{
    private readonly QdrantClient _client;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private const string CollectionName = "documents";

    public QdrantVectorStore(QdrantClient client, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        _client = client;
        _embeddingGenerator = embeddingGenerator;
    }

    public async Task UpsertChunksAsync(Guid documentId, IEnumerable<string> chunks, CancellationToken cancellationToken = default)
    {
        var chunkArray = chunks.ToArray();
        if (chunkArray.Length == 0) return;

        // دریافت دایمنشن وکتور به صورت داینامیک از اولین چانک
        var sampleEmbedding = await _embeddingGenerator.GenerateAsync(chunkArray[0], cancellationToken: cancellationToken);
        var vectorSize = (ulong)sampleEmbedding.Vector.Length;

        // بررسی و ساخت کالکشن در صورت عدم وجود
        if (!await _client.CollectionExistsAsync(CollectionName, cancellationToken: cancellationToken))
        {
            await _client.CreateCollectionAsync(
                CollectionName, 
                new VectorParams { Size = vectorSize, Distance = Distance.Cosine }, 
                cancellationToken: cancellationToken);
        }

        var points = new List<PointStruct>();

        foreach (var chunk in chunkArray)
        {
            var result = await _embeddingGenerator.GenerateAsync(chunk, cancellationToken: cancellationToken);
            var point = new PointStruct
            {
                Id = Guid.NewGuid(), // استفاده از Guid یکتا برای جلوگیری از Overwrite
                Vectors = result.Vector.ToArray(),
                Payload = { ["text"] = chunk, ["documentId"] = documentId.ToString() }
            };
            points.Add(point);
        }

        await _client.UpsertAsync(CollectionName, points, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<string>> SearchSimilarChunksAsync(string query, int topK = 3, CancellationToken cancellationToken = default)
    {
        var result = await _embeddingGenerator.GenerateAsync(query, cancellationToken: cancellationToken);
        var searchResults = await _client.SearchAsync(
            CollectionName,
            result.Vector.ToArray(), // [0] حذف شد
            limit: (ulong)topK,
            cancellationToken: cancellationToken);

        var chunks = new List<string>();
        foreach (var item in searchResults)
        {
            chunks.Add(item.Payload["text"].StringValue);
        }

        return chunks;
    }
}

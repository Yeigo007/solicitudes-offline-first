using System.Text.Json;

namespace Solicitudes.Application.Processing.Processors;

public class JsonRequestProcessor : IRequestProcessor
{
    public string Type => "JSON";

    public Task<string> ProcessAsync(
        string payload,
        CancellationToken cancellationToken = default)
    {
        using var document = JsonDocument.Parse(payload);

        var result = JsonSerializer.Serialize(
            document.RootElement,
            new JsonSerializerOptions
            {
                WriteIndented = true
            }
        );

        return Task.FromResult(result);
    }
}
using Solicitudes.Application.Processing;

namespace Solicitudes.Application.Processing.Processors;

public class TextRequestProcessor : IRequestProcessor
{
    public string Type => "TEXT";

    public Task<string> ProcessAsync(
        string payload,
        CancellationToken cancellationToken = default)
    {
        var result = payload.Trim().ToUpperInvariant();

        return Task.FromResult(result);
    }
}
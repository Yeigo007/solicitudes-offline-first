namespace Solicitudes.Application.Processing;

public interface IRequestProcessor
{
    string Type { get; }

    Task<string> ProcessAsync(
        string payload,
        CancellationToken cancellationToken = default
    );
}
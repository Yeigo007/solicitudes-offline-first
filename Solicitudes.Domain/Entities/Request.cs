
using Solicitudes.Domain.Enums;

namespace Solicitudes.Domain.Entities;

public sealed class Request
{
    public Guid Id { get; private set; }

    // Identificador generado por el frontend para evitar duplicados.
    public Guid ClientRequestId { get; private set; }

    public string Name { get; private set; }

    public string Payload { get; private set; }

    public RequestStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public string Type { get; private set; }

    public string? ProcessedPayload { get; private set; }

    public Request(
        Guid clientRequestId,
        string name,
        string payload,
        string type)
    {
        if (clientRequestId == Guid.Empty)
            throw new ArgumentException(
                "El identificador del cliente es obligatorio.",
                nameof(clientRequestId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "El nombre es obligatorio.",
                nameof(name));

        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException(
                "El contenido es obligatorio.",
                nameof(payload));

        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException(
                "El tipo es obligatorio.",
                nameof(type));

        Id = Guid.NewGuid();
        ClientRequestId = clientRequestId;
        Name = name;
        Payload = payload;
        Type = type;
        Status = RequestStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessed()
    {
        EnsurePending();
        Status = RequestStatus.Processed;
    }

    public void MarkAsFailed()
    {
        EnsurePending();
        Status = RequestStatus.Failed;
    }

    private void EnsurePending()
    {
        if (Status != RequestStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending requests can change their status.");
        }
    }

    public void CompleteProcessing(string processedPayload)
    {
        EnsurePending();

        if (string.IsNullOrWhiteSpace(processedPayload))
            throw new ArgumentException(
                "El resultado del procesamiento no puede estar vacío.",
                nameof(processedPayload));

        ProcessedPayload = processedPayload;
        Status = RequestStatus.Processed;
    }
}

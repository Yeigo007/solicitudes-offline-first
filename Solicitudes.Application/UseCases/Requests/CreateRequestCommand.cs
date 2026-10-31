namespace Solicitudes.Application.UseCases.Requests;

public record CreateRequestCommand(
    Guid ClientRequestId,
    string Name,
    string Payload,
    string Type);


namespace Solicitudes.Api.DTOs;

public sealed class CreateRequestRequest
{
    public Guid ClientRequestId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}

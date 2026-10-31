
using Solicitudes.Domain.Entities;
using Solicitudes.Domain.Enums;

namespace Solicitudes.Api.DTOs.Requests;

public sealed record RequestResponseDto(
    Guid Id,
    string Name,
    string Payload,
    string Type,
    RequestStatus Status,
    DateTime CreatedAt,
    string? ProcessedPayload
)
{
    public static RequestResponseDto FromEntity(Request request)
    {
        return new RequestResponseDto(
            request.Id,
            request.Name,
            request.Payload,
            request.Type,
            request.Status,
            request.CreatedAt,
            request.ProcessedPayload
        );
    }
}

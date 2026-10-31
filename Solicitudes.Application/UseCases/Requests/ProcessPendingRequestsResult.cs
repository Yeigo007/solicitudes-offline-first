namespace Solicitudes.Application.UseCases.Requests;

public record RequestProcessingFailure(
    Guid RequestId,
    string Error
);

public record ProcessPendingRequestsResult(
    int Total,
    int Processed,
    int Failed,
    IReadOnlyList<RequestProcessingFailure> Failures
);
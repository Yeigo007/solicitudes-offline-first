using Solicitudes.Domain.Entities;

namespace Solicitudes.Application.Abstractions;

public interface IRequestRepository
{
    Task AddAsync(
        Request request,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Request>> GetAllAsync(
        CancellationToken cancellationToken = default
    );

    Task<Request?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default
    );

    Task<Request?> GetByIdForUpdateAsync(
    Guid id,
    CancellationToken cancellationToken = default
    );

    Task UpdateAsync(
        Request request,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Guid>> GetPendingIdsAsync(
    CancellationToken cancellationToken = default
    );

    Task<Request?> GetByClientRequestIdAsync(
    Guid clientRequestId,
    CancellationToken cancellationToken = default);
}
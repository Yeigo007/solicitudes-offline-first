using Solicitudes.Application.Abstractions;
using Solicitudes.Domain.Entities;

namespace Solicitudes.Application.UseCases.Requests;

public class GetRequestByIdHandler
{
    private readonly IRequestRepository _repository;

    public GetRequestByIdHandler(IRequestRepository repository)
    {
        _repository = repository;
    }

    public Task<Request?> HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetByIdAsync(id, cancellationToken);
    }
}
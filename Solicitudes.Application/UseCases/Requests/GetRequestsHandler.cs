using Solicitudes.Application.Abstractions;
using Solicitudes.Domain.Entities;

namespace Solicitudes.Application.UseCases.Requests;

public class GetRequestsHandler
{
    private readonly IRequestRepository _repository;

    public GetRequestsHandler(IRequestRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<Request>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return _repository.GetAllAsync(cancellationToken);
    }
}
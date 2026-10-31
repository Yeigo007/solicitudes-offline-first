using Solicitudes.Application.Abstractions;
using Solicitudes.Application.Processing;
using Solicitudes.Domain.Entities;
using Solicitudes.Domain.Enums;

namespace Solicitudes.Application.UseCases.Requests;

public class ProcessRequestHandler
{
    private readonly IRequestRepository _repository;
    private readonly IEnumerable<IRequestProcessor> _processors;

    public ProcessRequestHandler(
        IRequestRepository repository,
        IEnumerable<IRequestProcessor> processors)
    {
        _repository = repository;
        _processors = processors;
    }

    public async Task<Request?> HandleAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        var request = await _repository.GetByIdForUpdateAsync(
            id,
            cancellationToken
        );

        if (request is null)
            return null;

        if (request.Status != RequestStatus.Pending)
        {
            throw new InvalidOperationException(
                "Solo se pueden procesar solicitudes pendientes."
            );
        }

        string result;

        try
        {
            var processor = _processors.FirstOrDefault(
                item => string.Equals(
                    item.Type,
                    request.Type,
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (processor is null)
            {
                throw new InvalidOperationException(
                    $"No existe un procesador para el tipo '{request.Type}'."
                );
            }

            result = await processor.ProcessAsync(
                request.Payload,
                cancellationToken
            );
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            request.MarkAsFailed();

            await _repository.UpdateAsync(
                request,
                cancellationToken
            );

            throw;
        }

        request.CompleteProcessing(result);

        await _repository.UpdateAsync(
            request,
            cancellationToken
        );

        return request;
    }
}
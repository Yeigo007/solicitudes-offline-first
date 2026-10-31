using Solicitudes.Application.Abstractions;

namespace Solicitudes.Application.UseCases.Requests;

public class ProcessPendingRequestsHandler
{
    private readonly IRequestRepository _repository;
    private readonly ProcessRequestHandler _processRequestHandler;

    public ProcessPendingRequestsHandler(
        IRequestRepository repository,
        ProcessRequestHandler processRequestHandler)
    {
        _repository = repository;
        _processRequestHandler = processRequestHandler;
    }

    public async Task<ProcessPendingRequestsResult> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var pendingIds = await _repository.GetPendingIdsAsync(
            cancellationToken
        );

        int processed = 0;
        int failed = 0;

        var failures = new List<RequestProcessingFailure>();

        foreach (var id in pendingIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var request = await _processRequestHandler.HandleAsync(
                    id,
                    cancellationToken
                );

                if (request is not null)
                {
                    processed++;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                failed++;

                failures.Add(new RequestProcessingFailure(
                    id,
                    ex.Message
                ));
            }
        }

        return new ProcessPendingRequestsResult(
            pendingIds.Count,
            processed,
            failed,
            failures
        );
    }
}
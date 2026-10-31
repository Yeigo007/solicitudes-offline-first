
using FluentValidation;
using Solicitudes.Application.Abstractions;
using Solicitudes.Application.Exceptions;
using Solicitudes.Domain.Entities;

namespace Solicitudes.Application.UseCases.Requests;

public sealed class CreateRequestHandler
{
    private readonly IRequestRepository _repository;
    private readonly IValidator<CreateRequestCommand> _validator;

    public CreateRequestHandler(
        IRequestRepository repository,
        IValidator<CreateRequestCommand> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Request> HandleAsync(
        CreateRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(
            command, cancellationToken);

        var existingRequest =
            await _repository.GetByClientRequestIdAsync(
                command.ClientRequestId,
                cancellationToken);

        if (existingRequest is not null)
            return existingRequest;

        var request = new Request(
            command.ClientRequestId,
            command.Name,
            command.Payload,
            command.Type);

        try
        {
            await _repository.AddAsync(request, cancellationToken);
            return request;
        }
        catch (DuplicateClientRequestException)
        {
            var duplicatedRequest =
                await _repository.GetByClientRequestIdAsync(
                    command.ClientRequestId,
                    cancellationToken);

            if (duplicatedRequest is not null)
                return duplicatedRequest;

            throw;
        }
    }
}


using Moq;
using Solicitudes.Application.Abstractions;
using Solicitudes.Application.Processing;
using Solicitudes.Application.Processing.Processors;
using Solicitudes.Application.UseCases.Requests;
using Solicitudes.Domain.Entities;
using Solicitudes.Domain.Enums;

namespace Solicitudes.Tests.UseCases.Requests;

public class ProcessPendingRequestsHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithSuccessfulAndFailedRequests_ProcessesBatchAndContinues()
    {
        // Arrange
        var repositoryMock = new Mock<IRequestRepository>();

        var textRequest = new Request(
            Guid.NewGuid(),
            "Solicitud texto",
            "hola mundo",
            "TEXT");

        var invalidJsonRequest = new Request(
            Guid.NewGuid(),
            "Solicitud JSON",
            "{ json invalido",
            "JSON");

        var requests = new Dictionary<Guid, Request>
        {
            [textRequest.Id] = textRequest,
            [invalidJsonRequest.Id] = invalidJsonRequest
        };

        repositoryMock
            .Setup(repository => repository.GetPendingIdsAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Guid>)new List<Guid>
            {
                textRequest.Id,
                invalidJsonRequest.Id
            });

        repositoryMock
            .Setup(repository => repository.GetByIdForUpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
                requests.TryGetValue(id, out var request)
                    ? request
                    : null);

        repositoryMock
            .Setup(repository => repository.UpdateAsync(
                It.IsAny<Request>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var processors = new IRequestProcessor[]
        {
            new TextRequestProcessor(),
            new JsonRequestProcessor()
        };

        var processRequestHandler = new ProcessRequestHandler(
            repositoryMock.Object,
            processors);

        var handler = new ProcessPendingRequestsHandler(
            repositoryMock.Object,
            processRequestHandler);

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Equal(2, result.Total);
        Assert.Equal(1, result.Processed);
        Assert.Equal(1, result.Failed);

        Assert.Single(result.Failures);
        Assert.Equal(
            invalidJsonRequest.Id,
            result.Failures[0].RequestId);

        Assert.Equal(RequestStatus.Processed, textRequest.Status);
        Assert.Equal("HOLA MUNDO", textRequest.ProcessedPayload);

        Assert.Equal(RequestStatus.Failed, invalidJsonRequest.Status);

        repositoryMock.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<Request>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task HandleAsync_WithNoPendingRequests_ReturnsZeroCounts()
    {
        // Arrange
        var repositoryMock = new Mock<IRequestRepository>();

        repositoryMock
            .Setup(repository => repository.GetPendingIdsAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Guid>)Array.Empty<Guid>());

        var processRequestHandler = new ProcessRequestHandler(
            repositoryMock.Object,
            Array.Empty<IRequestProcessor>());

        var handler = new ProcessPendingRequestsHandler(
            repositoryMock.Object,
            processRequestHandler);

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Equal(0, result.Total);
        Assert.Equal(0, result.Processed);
        Assert.Equal(0, result.Failed);
        Assert.Empty(result.Failures);
    }
}

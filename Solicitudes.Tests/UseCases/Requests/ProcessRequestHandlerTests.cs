
using System.Text.Json;
using Moq;
using Solicitudes.Application.Abstractions;
using Solicitudes.Application.Processing;
using Solicitudes.Application.Processing.Processors;
using Solicitudes.Application.UseCases.Requests;
using Solicitudes.Domain.Entities;
using Solicitudes.Domain.Enums;

namespace Solicitudes.Tests.UseCases.Requests;

public class ProcessRequestHandlerTests
{
    private readonly Mock<IRequestRepository> _repositoryMock;
    private readonly IEnumerable<IRequestProcessor> _processors;
    private readonly ProcessRequestHandler _handler;

    public ProcessRequestHandlerTests()
    {
        _repositoryMock = new Mock<IRequestRepository>();

        _processors = new IRequestProcessor[]
        {
            new TextRequestProcessor(),
            new JsonRequestProcessor()
        };

        _handler = new ProcessRequestHandler(
            _repositoryMock.Object,
            _processors);
    }

    [Fact]
    public async Task HandleAsync_WithTextRequest_ProcessesAndUpdatesRequest()
    {
        // Arrange
        var request = CreateRequest("TEXT", "  hola mundo  ");

        _repositoryMock
            .Setup(repository => repository.GetByIdForUpdateAsync(
                request.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        // Act
        var result = await _handler.HandleAsync(request.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RequestStatus.Processed, result.Status);
        Assert.Equal("HOLA MUNDO", result.ProcessedPayload);

        _repositoryMock.Verify(
            repository => repository.UpdateAsync(
                request,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithJsonRequest_ProcessesAndFormatsJson()
    {
        // Arrange
        var request = CreateRequest(
            "JSON",
            "{\"name\":\"Diego\",\"active\":true}");

        _repositoryMock
            .Setup(repository => repository.GetByIdForUpdateAsync(
                request.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        // Act
        var result = await _handler.HandleAsync(request.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RequestStatus.Processed, result.Status);
        Assert.NotNull(result.ProcessedPayload);

        using var document = JsonDocument.Parse(result.ProcessedPayload);

        Assert.Equal(
            "Diego",
            document.RootElement.GetProperty("name").GetString());

        Assert.True(
            document.RootElement.GetProperty("active").GetBoolean());

        Assert.Contains(Environment.NewLine, result.ProcessedPayload);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidJson_MarksRequestAsFailed()
    {
        // Arrange
        var request = CreateRequest("JSON", "{ json invalido");

        _repositoryMock
            .Setup(repository => repository.GetByIdForUpdateAsync(
                request.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        // Act
       await Assert.ThrowsAnyAsync<JsonException>(
    () => _handler.HandleAsync(request.Id));

        // Assert
        Assert.Equal(RequestStatus.Failed, request.Status);
        Assert.Null(request.ProcessedPayload);

        _repositoryMock.Verify(
            repository => repository.UpdateAsync(
                request,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithUnsupportedType_MarksRequestAsFailed()
    {
        // Arrange
        var request = CreateRequest("XML", "<root></root>");

        _repositoryMock
            .Setup(repository => repository.GetByIdForUpdateAsync(
                request.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.HandleAsync(request.Id));

        // Assert
        Assert.Contains("No existe un procesador", exception.Message);
        Assert.Equal(RequestStatus.Failed, request.Status);

        _repositoryMock.Verify(
            repository => repository.UpdateAsync(
                request,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonPendingRequest_ThrowsException()
    {
        // Arrange
        var request = CreateRequest("TEXT", "Hola");
        request.MarkAsProcessed();

        _repositoryMock
            .Setup(repository => repository.GetByIdForUpdateAsync(
                request.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.HandleAsync(request.Id));

        // Assert
        _repositoryMock.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<Request>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingRequest_ReturnsNull()
    {
        // Arrange
        var requestId = Guid.NewGuid();

        _repositoryMock
            .Setup(repository => repository.GetByIdForUpdateAsync(
                requestId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Request?)null);

        // Act
        var result = await _handler.HandleAsync(requestId);

        // Assert
        Assert.Null(result);

        _repositoryMock.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<Request>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Request CreateRequest(string type, string payload)
    {
        return new Request(
            Guid.NewGuid(),
            "Solicitud de prueba",
            payload,
            type);
    }
}

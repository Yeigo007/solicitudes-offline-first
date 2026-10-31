
using FluentValidation;
using Moq;
using Solicitudes.Application.Abstractions;
using Solicitudes.Application.Exceptions;
using Solicitudes.Application.UseCases.Requests;
using Solicitudes.Application.Validators;
using Solicitudes.Domain.Entities;
using Solicitudes.Domain.Enums;

namespace Solicitudes.Tests.UseCases.Requests;

public class CreateRequestHandlerTests
{
    private readonly Mock<IRequestRepository> _repositoryMock;
    private readonly CreateRequestCommandValidator _validator;
    private readonly CreateRequestHandler _handler;

    public CreateRequestHandlerTests()
    {
        _repositoryMock = new Mock<IRequestRepository>();
        _validator = new CreateRequestCommandValidator();

        _handler = new CreateRequestHandler(
            _repositoryMock.Object,
            _validator);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_CreatesRequest()
    {
        // Arrange
        var command = CreateValidCommand();

        _repositoryMock
            .Setup(repository =>
                repository.GetByClientRequestIdAsync(
                    command.ClientRequestId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((Request?)null);

        _repositoryMock
            .Setup(repository =>
                repository.AddAsync(
                    It.IsAny<Request>(),
                    It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.ClientRequestId, result.ClientRequestId);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.Payload, result.Payload);
        Assert.Equal(command.Type, result.Type);
        Assert.Equal(RequestStatus.Pending, result.Status);

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<Request>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithExistingClientRequestId_ReturnsExistingRequest()
    {
        // Arrange
        var command = CreateValidCommand();

        var existingRequest = new Request(
            command.ClientRequestId,
            command.Name,
            command.Payload,
            command.Type);

        _repositoryMock
            .Setup(repository =>
                repository.GetByClientRequestIdAsync(
                    command.ClientRequestId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRequest);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(existingRequest.Id, result.Id);
        Assert.Equal(existingRequest.ClientRequestId, result.ClientRequestId);

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<Request>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryDetectsDuplicate_ReturnsExistingRequest()
    {
        // Arrange
        var command = CreateValidCommand();

        var existingRequest = new Request(
            command.ClientRequestId,
            command.Name,
            command.Payload,
            command.Type);

        _repositoryMock
            .SetupSequence(repository =>
                repository.GetByClientRequestIdAsync(
                    command.ClientRequestId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((Request?)null)
            .ReturnsAsync(existingRequest);

        _repositoryMock
            .Setup(repository =>
                repository.AddAsync(
                    It.IsAny<Request>(),
                    It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateClientRequestException());

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(existingRequest.Id, result.Id);

        _repositoryMock.Verify(
            repository => repository.GetByClientRequestIdAsync(
                command.ClientRequestId,
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task HandleAsync_WithInvalidName_ThrowsValidationException()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Name = ""
        };

        // Act
        await Assert.ThrowsAsync<ValidationException>(
            () => _handler.HandleAsync(command));

        // Assert
        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<Request>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNameLongerThan150Characters_ThrowsValidationException()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Name = new string('A', 151)
        };

        // Act
        await Assert.ThrowsAsync<ValidationException>(
            () => _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WithInvalidType_ThrowsValidationException()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Type = "XML"
        };

        // Act
        await Assert.ThrowsAsync<ValidationException>(
            () => _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WithEmptyPayload_ThrowsValidationException()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Payload = ""
        };

        // Act
        await Assert.ThrowsAsync<ValidationException>(
            () => _handler.HandleAsync(command));
    }

    private static CreateRequestCommand CreateValidCommand()
    {
        return new CreateRequestCommand(
            Guid.NewGuid(),
            "Solicitud de prueba",
            "Hola mundo",
            "TEXT");
    }
}

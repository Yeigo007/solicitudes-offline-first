
using Solicitudes.Domain.Entities;
using Solicitudes.Domain.Enums;

namespace Solicitudes.Tests.Domain;

public class RequestTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesPendingRequest()
    {
        // Arrange
        var clientRequestId = Guid.NewGuid();

        // Act
        var request = new Request(
            clientRequestId,
            "Solicitud de prueba",
            "Hola mundo",
            "TEXT");

        // Assert
        Assert.NotEqual(Guid.Empty, request.Id);
        Assert.Equal(clientRequestId, request.ClientRequestId);
        Assert.Equal("Solicitud de prueba", request.Name);
        Assert.Equal("Hola mundo", request.Payload);
        Assert.Equal("TEXT", request.Type);
        Assert.Equal(RequestStatus.Pending, request.Status);
        Assert.True(request.CreatedAt <= DateTime.UtcNow);
        Assert.Null(request.ProcessedPayload);
    }

    [Fact]
    public void Constructor_WithEmptyClientRequestId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Request(
                Guid.Empty,
                "Solicitud",
                "Contenido",
                "TEXT"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ThrowsException(string? name)
    {
        Assert.Throws<ArgumentException>(() =>
            new Request(
                Guid.NewGuid(),
                name!,
                "Contenido",
                "TEXT"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidPayload_ThrowsException(string? payload)
    {
        Assert.Throws<ArgumentException>(() =>
            new Request(
                Guid.NewGuid(),
                "Solicitud",
                payload!,
                "TEXT"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidType_ThrowsException(string? type)
    {
        Assert.Throws<ArgumentException>(() =>
            new Request(
                Guid.NewGuid(),
                "Solicitud",
                "Contenido",
                type!));
    }

    [Fact]
    public void CompleteProcessing_WithValidPayload_ChangesStatusToProcessed()
    {
        // Arrange
        var request = CreateRequest();

        // Act
        request.CompleteProcessing("HOLA MUNDO");

        // Assert
        Assert.Equal(RequestStatus.Processed, request.Status);
        Assert.Equal("HOLA MUNDO", request.ProcessedPayload);
    }

    [Fact]
    public void MarkAsFailed_ChangesStatusToFailed()
    {
        var request = CreateRequest();

        request.MarkAsFailed();

        Assert.Equal(RequestStatus.Failed, request.Status);
    }

    [Fact]
    public void MarkAsProcessed_ChangesStatusToProcessed()
    {
        var request = CreateRequest();

        request.MarkAsProcessed();

        Assert.Equal(RequestStatus.Processed, request.Status);
    }

    [Fact]
    public void CompleteProcessing_WithEmptyPayload_ThrowsException()
    {
        var request = CreateRequest();

        Assert.Throws<ArgumentException>(() =>
            request.CompleteProcessing(" "));
    }

    [Fact]
    public void MarkAsProcessed_WhenAlreadyProcessed_ThrowsException()
    {
        var request = CreateRequest();

        request.MarkAsProcessed();

        Assert.Throws<InvalidOperationException>(() =>
            request.MarkAsProcessed());
    }

    [Fact]
    public void MarkAsFailed_WhenAlreadyProcessed_ThrowsException()
    {
        var request = CreateRequest();

        request.MarkAsProcessed();

        Assert.Throws<InvalidOperationException>(() =>
            request.MarkAsFailed());
    }

    private static Request CreateRequest()
    {
        return new Request(
            Guid.NewGuid(),
            "Solicitud de prueba",
            "Contenido de prueba",
            "TEXT");
    }
}

using Solicitudes.Application.Abstractions;
using Solicitudes.Application.UseCases.Requests;
using Solicitudes.Infrastructure;
using Solicitudes.Application.Processing;
using Solicitudes.Application.Processing.Processors;
using Solicitudes.Application;
using Solicitudes.Api.Middleware;
using Solicitudes.Api.DTOs.Requests;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddScoped<CreateRequestHandler>();
builder.Services.AddScoped<GetRequestsHandler>();
builder.Services.AddScoped<GetRequestByIdHandler>();
builder.Services.AddScoped<IRequestProcessor, TextRequestProcessor>();
builder.Services.AddScoped<IRequestProcessor, JsonRequestProcessor>();
builder.Services.AddScoped<ProcessRequestHandler>();
builder.Services.AddScoped<ProcessPendingRequestsHandler>();


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// politicas CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("Frontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

        
    
    app.MapPost("/api/requests", async (
        CreateRequestRequest request,
        CreateRequestHandler handler,
        CancellationToken cancellationToken) =>
    {
        var command = new CreateRequestCommand(
            request.ClientRequestId,
            request.Name,
            request.Payload,
            request.Type);

        var createdRequest = await handler.HandleAsync(
            command,
            cancellationToken
        );

        var response = RequestResponseDto.FromEntity(createdRequest);

        return Results.Created(
            $"/api/requests/{response.Id}",
            response
        );
    })
    .WithName("CreateRequest");



    app.MapGet("/api/requests", async (
    GetRequestsHandler handler,
    CancellationToken cancellationToken) =>
    {
        var requests = await handler.HandleAsync(cancellationToken);

        var response = requests
            .Select(RequestResponseDto.FromEntity)
            .ToList();

        return Results.Ok(response);
    })
    .WithName("GetRequests");

    app.MapGet("/api/requests/{id:guid}", async (
    Guid id,
    GetRequestByIdHandler handler,
    CancellationToken cancellationToken) =>
    {
        var request = await handler.HandleAsync(id, cancellationToken);

        if (request is null)
            return Results.NotFound();

        var response = RequestResponseDto.FromEntity(request);

        return Results.Ok(response);
    })
    .WithName("GetRequestById");

    app.MapPost("/api/requests/{id:guid}/process", async (
    Guid id,
    ProcessRequestHandler handler,
    CancellationToken cancellationToken) =>
    {
        try
        {
            var request = await handler.HandleAsync(id, cancellationToken);

            if (request is null)
                return Results.NotFound();

            var response = RequestResponseDto.FromEntity(request);

            return Results.Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (JsonException)
        {
            return Results.BadRequest(new
            {
                message = "El contenido enviado no es un JSON válido."
            });
        }
    })
    .WithName("ProcessRequest");

    app.MapPost("/api/requests/process-pending", async (
    ProcessPendingRequestsHandler handler,
    CancellationToken cancellationToken) =>
    {
        var result = await handler.HandleAsync(cancellationToken);

        return Results.Ok(result);
    })
    .WithName("ProcessPendingRequests");

    app.Run();

    public record CreateRequestRequest(
        Guid ClientRequestId,
        string Name,
        string Payload,
        string Type
    );

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public partial class Program { }
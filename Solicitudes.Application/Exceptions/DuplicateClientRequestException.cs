namespace Solicitudes.Application.Exceptions;

public sealed class DuplicateClientRequestException : Exception
{
    public DuplicateClientRequestException()
        : base("Ya existe una solicitud con ese identificador de cliente.")
    {
    }
}

using Microsoft.EntityFrameworkCore;
using Solicitudes.Application.Abstractions;
using Solicitudes.Domain.Entities;
using Solicitudes.Infrastructure.Persistence;
using Solicitudes.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Solicitudes.Application.Exceptions;

namespace Solicitudes.Infrastructure.Repositories;

public class RequestRepository : IRequestRepository
{
    private readonly SolicitudesDbContext _context;

    public RequestRepository(SolicitudesDbContext context)
    {
        _context = context;
    }

    
    
    public async Task AddAsync(
        Request request,
        CancellationToken cancellationToken = default)
    {
        await _context.Requests.AddAsync(request, cancellationToken);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException postgresException
                && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
                && postgresException.ConstraintName == "IX_Requests_ClientRequestId")
        {
            _context.Entry(request).State = EntityState.Detached;

            throw new DuplicateClientRequestException();
        }
    }


    public async Task<IReadOnlyList<Request>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .AsNoTracking()
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Request?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .AsNoTracking()
            .FirstOrDefaultAsync(
                request => request.Id == id,
                cancellationToken
            );
    }

    public async Task<Request?> GetByIdForUpdateAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .FirstOrDefaultAsync(
                request => request.Id == id,
                cancellationToken
            );
    }

    public async Task UpdateAsync(
    Request request,
    CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
    

    public async Task<IReadOnlyList<Guid>> GetPendingIdsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Where(request => request.Status == RequestStatus.Pending)
            .OrderBy(request => request.CreatedAt)
            .Select(request => request.Id)
            .Take(100)
            .ToListAsync(cancellationToken);
    }

    public async Task<Request?> GetByClientRequestIdAsync(
    Guid clientRequestId,
    CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .AsNoTracking()
            .FirstOrDefaultAsync(
                request => request.ClientRequestId == clientRequestId,
                cancellationToken);
    }
}
    
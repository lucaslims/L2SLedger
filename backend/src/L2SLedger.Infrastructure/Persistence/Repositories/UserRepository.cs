using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação do repositório de usuários.
/// Conforme ADR-020 (Clean Architecture), ADR-029 (soft delete).
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly L2SLedgerDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(
        L2SLedgerDbContext context,
        ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adicionando usuário {Email}", user.Email);
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Usuário criado com ID {UserId}", user.Id);
        
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Atualizando usuário {UserId}", user.Id);
        
        user.UpdateTimestamp();
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deletando usuário {UserId}", user.Id);
        
        user.MarkAsDeleted();
        await _context.SaveChangesAsync(cancellationToken);
    }
}

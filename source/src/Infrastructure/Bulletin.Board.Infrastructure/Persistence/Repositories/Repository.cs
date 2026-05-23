using System.Linq.Expressions;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bulletin.Board.Infrastructure.Persistence.Repositories;

public class Repository<T>(ApplicationDbContext context) : IRepository<T> where T : class
{
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await DbSet.FindAsync([id], ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await DbSet.ToListAsync(ct);

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await DbSet.Where(predicate).ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await DbSet.AddAsync(entity, ct);

    public void Update(T entity) => DbSet.Update(entity);

    public void Remove(T entity) => DbSet.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);
}

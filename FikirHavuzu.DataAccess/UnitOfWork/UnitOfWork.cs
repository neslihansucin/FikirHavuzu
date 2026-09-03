using FikirHavuzu.DataAccess.Context;
using FikirHavuzu.DataAccess.Repositories;
using FikirHavuzu.Entity.Entities;

namespace FikirHavuzu.DataAccess.UnitOfWork;


public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly Dictionary<Type, object> _repositories;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        _repositories = new Dictionary<Type, object>();
    }

    public IGenericRepository<T> GetRepository<T>() where T : BaseEntity
    {
        var type = typeof(T);

        if (!_repositories.ContainsKey(type))
        {
            var repositoryInstance = new GenericRepository<T>(_context);
            _repositories.Add(type, repositoryInstance);
        }

        return (IGenericRepository<T>)_repositories[type];
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}
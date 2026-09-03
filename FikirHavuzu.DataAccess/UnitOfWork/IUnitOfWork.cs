using FikirHavuzu.DataAccess.Repositories;
using FikirHavuzu.Entity.Entities;

namespace FikirHavuzu.DataAccess.UnitOfWork;


public interface IUnitOfWork : IAsyncDisposable
{
    IGenericRepository<T> GetRepository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync();
}
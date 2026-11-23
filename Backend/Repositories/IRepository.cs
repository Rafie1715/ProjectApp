using Microsoft.EntityFrameworkCore.Storage;

namespace CutiApp.Repositories;

public interface IRepository<T> where T : class
{
    IQueryable<T> Query();
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}


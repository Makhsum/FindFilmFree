using FindFilmFree.Application.Abstraction.Interfaces;
using FindFilmFree.Domain.Models;
using FindFilmFree.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FindFilmFree.Application.Repository;

public abstract class Repository<TEntity>:IRepository<TEntity> where TEntity:EntityBase
{
    protected readonly DatabaseContext _context;
    protected DbSet<TEntity> _dbSet;

    public Repository(DatabaseContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    } 
    
    public async virtual Task<bool> AddAsync(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public  async virtual  Task<TEntity?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public  async virtual  Task<TEntity?> GetByTelegramIdAsync(long telegramId)
    {
        throw new NotImplementedException();
    }

    public  async virtual  Task<bool> Remove(int id)
    {
        throw new NotImplementedException();
    }

    public  async virtual  Task<bool> UpdateAsync(TEntity updatedEntity)
    {
        throw new NotImplementedException();
    }
}
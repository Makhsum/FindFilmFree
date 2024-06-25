using FindFilmFree.Domain.Models;

namespace FindFilmFree.Application.Abstraction.Interfaces;

public interface IRepository<TEntity> where TEntity:EntityBase
{
    Task<bool> AddAsync(TEntity entity);
    Task<TEntity?> GetByIdAsync(int id);
    Task<TEntity?> GetByTelegramIdAsync(long telegramId);
    Task<bool> Remove(int id);
    Task<bool> UpdateAsync(TEntity updatedEntity);

}
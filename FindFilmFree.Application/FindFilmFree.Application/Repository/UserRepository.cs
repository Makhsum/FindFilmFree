using FindFilmFree.Application.Abstraction.Interfaces;
using FindFilmFree.Domain.Models;
using FindFilmFree.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FindFilmFree.Application.Repository;

public class UserRepository:Repository<User>,IUserRepository
{
    public UserRepository(DatabaseContext context) : base(context)
    {
        
    }

    public override async Task<bool> AddAsync(User entity)
    {
        try
        {
            await _dbSet.AddAsync(entity);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public override async Task<bool> Remove(int id)
    {
        try
        {
            var user = await _dbSet.FirstOrDefaultAsync(u => u.Id == id);
            if (user!=null)
            {
                _dbSet.Remove(user);
                return true;
            }
            
                return false;
            
           
        }
        catch (Exception e)
        {
            return false;
        }
       
    }

    public override async Task<bool> UpdateAsync(User updatedEntity)
    {
        try
        { 
            var oldUser = await _dbSet.FirstOrDefaultAsync(u => u.Id == updatedEntity.Id);
            if (oldUser!=null)
            {
                oldUser.UserName = updatedEntity.UserName;
                oldUser.Name = updatedEntity.Name;
                oldUser.LastName = updatedEntity.LastName;
                oldUser.IsActive = updatedEntity.IsActive;
                oldUser.IsModer = updatedEntity.IsModer;
                return true;
            }
           
                return false;
           

        }
        catch (Exception e)
        {
            return false;
        }
      
        // return 
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<IEnumerable<User>> GetAllPaginationAsync(int skip, int take)
    {
        return await _dbSet.Skip(skip).Take(take).ToListAsync();
    }

    public async Task<int> GetUsersCount() => await  _dbSet.CountAsync();
    public async Task<User?> GetByUserName(string username) => await _dbSet.FirstOrDefaultAsync(u => u.UserName == username);
    


    public override async Task<User?> GetByIdAsync(int id)
    {
        try
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (Exception e)
        {
            return null;
        }
    }

  

    public override async Task<User?> GetByTelegramIdAsync(long telegramId)
    {
        try
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.TelegramId == telegramId);
        }
        catch (Exception e)
        {
            return null;
        }
    }
}
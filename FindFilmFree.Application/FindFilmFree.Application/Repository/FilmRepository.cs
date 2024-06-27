using FindFilmFree.Application.Abstraction.Interfaces;
using FindFilmFree.Domain.Models;
using FindFilmFree.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FindFilmFree.Application.Repository;

public class FilmRepository:Repository<Film>,IFilmRepository
{
    public FilmRepository(DatabaseContext context) : base(context)
    {
        
    }

    public async Task<int> FilmsCount()
    {
        await Task.Delay(1);
        return _dbSet.Count();
    }

    public  async Task<Film?> GetByNumber(int number)
    {
        return await _dbSet.FirstOrDefaultAsync(f => f.Number == number);
    }

    public async Task<bool> DeleteByNumber(int number)
    {
        var film = await _dbSet.FirstOrDefaultAsync(f => f.Number == number);
        if (film!=null)
        {
            _dbSet.Remove(film);
            return true;
        }

        return false;
    }

    public override async Task<bool> AddAsync(Film entity)
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
            try
            {

            }
            catch (Exception e)
            {
                Console.WriteLine("");
            }
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

    public override async Task<bool> UpdateAsync(Film updatedEntity)
    {
        try
        { 
            var oldUser = await _dbSet.FirstOrDefaultAsync(u => u.Id == updatedEntity.Id);
            if (oldUser!=null)
            {
                oldUser.Name = updatedEntity.Name;
                oldUser.Link = updatedEntity.Link;
                oldUser.Number = updatedEntity.Number;
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
    
    public override async Task<Film?> GetByIdAsync(int id)
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
    
}
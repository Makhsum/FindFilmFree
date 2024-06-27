using FindFilmFree.Domain.Models;

namespace FindFilmFree.Application.Abstraction.Interfaces;

public interface IUserRepository:IRepository<User>
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetAllPaginationAsync(int skip,int take);
    Task<int> GetUsersCount();
    Task<User?> GetByUserName(string username);
}
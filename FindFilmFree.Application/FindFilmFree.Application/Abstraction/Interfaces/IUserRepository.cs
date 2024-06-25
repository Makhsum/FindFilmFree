using FindFilmFree.Domain.Models;

namespace FindFilmFree.Application.Abstraction.Interfaces;

public interface IUserRepository:IRepository<User>
{
    Task<IEnumerable<User>> GetAllAsync();
}
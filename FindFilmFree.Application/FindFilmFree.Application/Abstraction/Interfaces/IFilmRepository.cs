using FindFilmFree.Domain.Models;

namespace FindFilmFree.Application.Abstraction.Interfaces;

public interface IFilmRepository:IRepository<Film>
{
    Task<int> FilmsCount();
    Task<Film?> GetByNumber(int number);
}
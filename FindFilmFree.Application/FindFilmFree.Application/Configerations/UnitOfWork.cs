using FindFilmFree.Application.Abstraction.Interfaces;
using FindFilmFree.Application.Repository;
using FindFilmFree.Infrastructure.Database;

namespace FindFilmFree.Application.Configerations;

public class UnitOfWork:IUnitOfWork
{
    private readonly DatabaseContext _context;
    public IUserRepository Users { get; }
    public IFilmRepository Films { get; }

    public UnitOfWork(DatabaseContext context)
    {
        _context = context;
        Users = new UserRepository(_context);
        Films = new FilmRepository(_context);
    }
    public async Task CompleteAsync()
    {
        await _context.SaveChangesAsync();
    }
}
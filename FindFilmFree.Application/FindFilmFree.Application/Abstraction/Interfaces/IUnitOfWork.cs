namespace FindFilmFree.Application.Abstraction.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IFilmRepository Films { get; }
    Task CompleteAsync();
}
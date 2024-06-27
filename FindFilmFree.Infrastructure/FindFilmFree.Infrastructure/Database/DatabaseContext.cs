using FindFilmFree.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace FindFilmFree.Infrastructure.Database;

public class DatabaseContext:DbContext
{
    private readonly string _connectionstring;
    public DbSet<User> Users { get; set; }
    public DbSet<Film> Films { get; set; }
    public DatabaseContext(string connectionstring)
    {
        _connectionstring = connectionstring;
        // Database.EnsureDeleted();
        Database.EnsureCreated();
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_connectionstring);
    }
}
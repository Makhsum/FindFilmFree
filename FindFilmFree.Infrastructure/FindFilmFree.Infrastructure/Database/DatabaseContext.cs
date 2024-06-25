using FindFilmFree.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace FindFilmFree.Infrastructure.Database;

public class DatabaseContext:DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Film> Films { get; set; }
    public DatabaseContext()
    {
      //  Database.EnsureDeleted();
        Database.EnsureCreated();
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source= findfilmdatabase.db");
    }
}
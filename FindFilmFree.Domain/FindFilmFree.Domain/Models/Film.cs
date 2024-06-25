namespace FindFilmFree.Domain.Models;

public class Film:EntityBase
{

    public Guid FilmId { get; set; }
    public int Number { get; set; }
    public string Name { get; set; }
    public string Link { get; set; }
}
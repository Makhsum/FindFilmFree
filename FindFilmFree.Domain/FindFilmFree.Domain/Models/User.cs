namespace FindFilmFree.Domain.Models;

public class User:EntityBase
{
    public long TelegramId { get; set; }
    public string Name { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
    public long ReferrerId { get; set; }
    public long FriendReferrerId { get; set; }
    public string Language { get; set; }
    public bool IsActive { get; set; }
    public bool IsModer { get; set; }
}
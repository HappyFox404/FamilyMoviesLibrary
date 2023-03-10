namespace FamilyMoviesLibrary.Context.Models;

public class User : BaseData
{
    public long TelegramId { get; set; }

    public string? TelegramUserName { get; set; }

    public Guid? GroupId { get; set; }
    public Group? Group { get; set; }
    
    public Message? Message { get; set; }
}
namespace FamilyMoviesLibrary.Context.Models;

public class User : BaseData
{
    public Guid Id { get; set; }
    public Guid TelegramId { get; set; }

    public Guid? GroupId { get; set; }
    public Group? Group { get; set; }
}
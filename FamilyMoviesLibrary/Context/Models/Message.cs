namespace FamilyMoviesLibrary.Context.Models;

public class Message : BaseData
{
    public Guid Id { get; set; }
    public string? Data { get; set; }
    public bool IsNeedAdditionalMessage { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
}
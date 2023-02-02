namespace FamilyMoviesLibrary.Context.Models;

public class Group : BaseData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    
    public List<User> Users { get; set; } = new();
}
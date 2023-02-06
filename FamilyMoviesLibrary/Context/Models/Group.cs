namespace FamilyMoviesLibrary.Context.Models;

public class Group : BaseData
{
    public string Name { get; set; } = null!;
    
    public List<User> Users { get; set; } = new();
    public List<Film> Films { get; set; } = new();
}
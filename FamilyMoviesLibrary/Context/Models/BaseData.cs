namespace FamilyMoviesLibrary.Context.Models;

public class BaseData
{
    public Guid Id { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.Now;
}
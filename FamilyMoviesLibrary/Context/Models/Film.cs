namespace FamilyMoviesLibrary.Context.Models;

public class Film : BaseData
{
    public long KinopoiskId { get; set; }
    public int Rate { get; set; }

    public Guid GroupId { get; set; }
    public Group Group { get; set; }
}
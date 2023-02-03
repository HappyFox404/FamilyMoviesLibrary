namespace FamilyMoviesLibrary.Models.Atributes;

[AttributeUsage(AttributeTargets.Class)]
public class ApplicationCommandAttribute : Attribute
{
    public bool IsDefault { get; }

    public ApplicationCommandAttribute()
    {
        IsDefault = false;
    }
    
    public ApplicationCommandAttribute(bool state)
    {
        IsDefault = state;
    }
}
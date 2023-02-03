namespace FamilyMoviesLibrary.Models.Atributes;

[AttributeUsage(AttributeTargets.Class)]
public class BotCommandAttribute : Attribute
{
    public bool IsDefault { get; }

    public BotCommandAttribute()
    {
        IsDefault = false;
    }
    
    public BotCommandAttribute(bool state)
    {
        IsDefault = state;
    }
}
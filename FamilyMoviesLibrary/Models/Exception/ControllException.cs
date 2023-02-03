namespace FamilyMoviesLibrary.Models.Exception;

public class ControllException : System.Exception
{
    public readonly string NormalMessage;
    public ControllException(string message) : base(message)
    {
        NormalMessage = message;
    }
}
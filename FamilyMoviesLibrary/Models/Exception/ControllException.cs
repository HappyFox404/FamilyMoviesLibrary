namespace FamilyMoviesLibrary.Models.Exception;

public class ControllException : System.Exception
{
    public readonly string NormalMessage;
    public readonly bool UserAnswer;
    public ControllException(string message, bool userAnswer = true) : base(message)
    {
        NormalMessage = message;
        UserAnswer = userAnswer;
    }
}
namespace Xabbo.Command;

public class InvalidArgsException : Exception
{
    public InvalidArgsException() { }

    public InvalidArgsException(string message)
        : base(message)
    { }
}

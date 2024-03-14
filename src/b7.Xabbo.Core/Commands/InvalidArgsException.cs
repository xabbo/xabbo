using System;

namespace b7.Xabbo.Commands;

public class InvalidArgsException : Exception
{
    public InvalidArgsException() { }

    public InvalidArgsException(string message)
        : base(message)
    { }
}

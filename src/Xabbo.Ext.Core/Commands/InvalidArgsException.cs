using System;

namespace Xabbo.Ext.Commands;

public class InvalidArgsException : Exception
{
    public InvalidArgsException() { }

    public InvalidArgsException(string message)
        : base(message)
    { }
}

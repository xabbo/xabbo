namespace Xabbo.Exceptions;

public sealed class OperationInProgressException(string operationName)
    : Exception("An operation is already in progress.")
{
    public string OperationName { get; } = operationName;
}

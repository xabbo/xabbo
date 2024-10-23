namespace Xabbo.Exceptions;

public sealed class OperationInProgressException(string operationName)
    : Exception($"An operation is currently in progress: '{operationName}'.")
{
    public string OperationName { get; } = operationName;
}

using System.Diagnostics.CodeAnalysis;

using Xabbo.Core;

namespace Xabbo.Ext.Core.Services;

public interface IFigureConverterService
{
    event Action Available;
    bool IsAvailable { get; }
    bool TryConvertToModern(string originsFigureString, [NotNullWhen(true)] out Figure? figure);
}
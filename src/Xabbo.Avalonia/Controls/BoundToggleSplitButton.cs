using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Xabbo.Avalonia.Controls;

public class BoundToggleSplitButton : ToggleSplitButton
{
    protected override void OnClickPrimary(RoutedEventArgs? e)
    {
        (var command, var parameter) = (Command, CommandParameter);

        if (IsEffectivelyEnabled)
        {
            var eventArgs = new RoutedEventArgs(ClickEvent);
            RaiseEvent(eventArgs);

            if (!eventArgs.Handled && command?.CanExecute(parameter) == true)
            {
                command.Execute(parameter);
                eventArgs.Handled = true;
            }
        }
    }
}
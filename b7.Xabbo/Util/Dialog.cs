using System;
using System.Windows;

namespace b7
{
    public static class Dialog
    {
        const string APP_NAME = "xabbo";

        public static MessageBoxResult Show(string message, string caption, MessageBoxButton button, MessageBoxImage image)
            => MessageBox.Show(message, caption, button, image);

        public static void ShowOK(string message, string caption, MessageBoxImage image)
            => Show(message, caption, MessageBoxButton.OK, image);

        public static void ShowInfo(string message, string caption = APP_NAME)
            => ShowOK(message, caption, MessageBoxImage.Information);

        public static void ShowWarning(string message, string caption = APP_NAME)
            => ShowOK(message, caption, MessageBoxImage.Warning);

        public static void ShowError(string message, string caption = APP_NAME)
            => ShowOK(message, caption, MessageBoxImage.Error);

        public static bool ConfirmYesNoWarning(string message, string caption = APP_NAME)
            => Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
    }
}

using System.Reflection;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;

using Xabbo.ViewModels;

namespace Xabbo.Utility;

public static class DialogExtensions
{
    public static Task<ContentDialogResult> ShowAsync(this IDialogService dialogService,
        string title,
        string content,
        string primary = "OK",
        string secondary = ""
    )
    {
        return dialogService.ShowContentDialogAsync(dialogService.CreateViewModel<MainViewModel>(),
            new HanumanInstitute.MvvmDialogs.Avalonia.Fluent.ContentDialogSettings
            {
                Title = title,
                Content = content,
                PrimaryButtonText = primary,
                SecondaryButtonText = secondary,
            });
    }
}
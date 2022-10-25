using System;
using System.Windows;

using Microsoft.Extensions.DependencyInjection;

using Wpf.Ui.Mvvm.Contracts;

namespace b7.Xabbo.Services;

public class PageService : IPageService
{
    private readonly IServiceProvider _services;

    public PageService(IServiceProvider services)
    {
        _services = services;
    }

    public T? GetPage<T>() where T : class
    {
        return _services.GetRequiredService<T>();
    }

    public FrameworkElement? GetPage(Type pageType)
    {
        return _services.GetService(pageType) as FrameworkElement;
    }
}

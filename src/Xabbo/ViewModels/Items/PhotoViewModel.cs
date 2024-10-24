using Xabbo.Core;

namespace Xabbo.ViewModels;

public sealed class PhotoViewModel(IItem item, Lazy<Task<string?>> getPhotoUrl) : ItemViewModelBase(item)
{
    private readonly Lazy<Task<string?>> _getPhotoUrlAsync = getPhotoUrl;
    public Task<string?> PhotoUrl => _getPhotoUrlAsync.Value;
}
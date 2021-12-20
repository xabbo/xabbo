using System;
using System.Threading.Tasks;

using Xabbo.Core.GameData;

using b7.Xabbo.Events;

namespace b7.Xabbo.Services
{
    public interface IGameDataManager
    {
        FigureData? FigureData { get; }
        FurniData? FurniData { get; }
        ProductData? ProductData { get; }
        ExternalTexts? ExternalTexts { get; }

        event EventHandler<GameDataEventArgs>? MetadataLoaded;
        event EventHandler<GameDataEventArgs>? MetadataLoadFailed;

        Task InitializeAsync();
        Task LoadAsync();

        Task<FigureData> GetFigureDataAsync();
        Task<FurniData> GetFurniDataAsync();
        Task<ProductData> GetProductDataAsync();
        Task<ExternalTexts> GetExternalTextsAsync();
    }
}

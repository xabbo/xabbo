using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Xabbo.Core.GameData;
using Xabbo.Core.Web;

using b7.Xabbo.Events;
using Xabbo.Core;

namespace b7.Xabbo.Services
{
    public class GameDataManager : IGameDataManager
    {
        private readonly string _cachePath;
        private readonly IUriProvider<HabboEndpoints> _uriProvider;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly TaskCompletionSource<FigureData> _tcsFigureData;
        private readonly TaskCompletionSource<FurniData> _tcsFurniData;
        private readonly TaskCompletionSource<ProductData> _tcsProductData;
        private readonly TaskCompletionSource<ExternalTexts> _tcsExternalTexts;

        private bool _isInitialized;

        public FigureData? FigureData { get; private set; }
        public FurniData? FurniData { get; private set; }
        public ProductData? ProductData { get; private set; }
        public ExternalTexts? ExternalTexts { get; private set; }

        public event EventHandler<GameDataEventArgs>? MetadataLoaded;
        public event EventHandler<GameDataEventArgs>? MetadataLoadFailed;

        public GameDataManager(
            IUriProvider<HabboEndpoints> uriProvider,
            IHttpClientFactory httpClientFactory)
        {
            _cachePath = "cache";
            _uriProvider = uriProvider;
            _httpClientFactory = httpClientFactory;

            _tcsFigureData = new TaskCompletionSource<FigureData>();
            _tcsFurniData = new TaskCompletionSource<FurniData>();
            _tcsProductData = new TaskCompletionSource<ProductData>();
            _tcsExternalTexts = new TaskCompletionSource<ExternalTexts>();
        }

        private string GetCacheDirectory(GameDataType type) => Path.Combine(_cachePath, type.ToString().ToLower());

        public Task LoadAsync() => throw new NotImplementedException();

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            foreach (GameDataType gameDataType in Enum.GetValues<GameDataType>())
                Directory.CreateDirectory(GetCacheDirectory(gameDataType));

            using HttpClient http = _httpClientFactory.CreateClient("Xabbo");
            HttpResponseMessage response = await http.GetAsync(_uriProvider[HabboEndpoints.GameDataHashes2]);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception("Failed to load game data hashes.");
            }

            string json = await response.Content.ReadAsStringAsync();
            GameDataHashesContainer hashesContainer = JsonSerializer.Deserialize<GameDataHashesContainer>(json)
                ?? throw new FormatException("Failed to deserialize game data hashes.");

            List<Task> loadTasks = new();

            foreach (GameDataHash gameDataInfo in hashesContainer.Hashes)
            {
                GameDataType gameDataType = gameDataInfo.Name switch
                {
                    "furnidata" => GameDataType.Furni,
                    "productdata" => GameDataType.Product,
                    "external_texts" => GameDataType.Texts,
                    "figurepartlist" => GameDataType.Figure,
                    _ => (GameDataType)(-1)
                };

                if (!Enum.IsDefined(gameDataType)) continue;

                loadTasks.Add(LoadMetadataAsync(http, gameDataType, gameDataInfo.Url + "/" + gameDataInfo.Hash));
            }

            await Task.WhenAll(loadTasks);

            XabboCoreExtensions.Initialize(
                await GetFurniDataAsync(),
                await GetExternalTextsAsync()
            );
        }

        private async Task LoadMetadataAsync(HttpClient client, GameDataType type, string url)
        {
            try
            {
                string cacheDirectory = GetCacheDirectory(type);
                string fileName = Path.GetFileNameWithoutExtension(url);
                string filePath = Path.Combine(cacheDirectory, fileName);

                if (!File.Exists(filePath))
                {
                    try
                    {
                        using Stream inStream = await client.GetStreamAsync(url);
                        using Stream outStream = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite);
                        await inStream.CopyToAsync(outStream);
                    }
                    catch
                    {
                        try { File.Delete(filePath); }
                        catch { }

                        throw;
                    }
                }

                switch (type)
                {
                    case GameDataType.Figure:
                        FigureData = FigureData.LoadXml(filePath);
                        _tcsFigureData.TrySetResult(FigureData);
                        break;
                    case GameDataType.Furni:
                        FurniData = FurniData.LoadJson(File.ReadAllText(filePath));
                        _tcsFurniData.TrySetResult(FurniData);
                        break;
                    case GameDataType.Product:
                        ProductData = ProductData.LoadJsonFile(filePath);
                        _tcsProductData.TrySetResult(ProductData);
                        break;
                    case GameDataType.Texts:
                        ExternalTexts = ExternalTexts.Load(filePath);
                        _tcsExternalTexts.TrySetResult(ExternalTexts);
                        break;
                    default: break;
                }

                MetadataLoaded?.Invoke(this, new GameDataEventArgs(type));
            }
            catch (Exception ex)
            {
                switch (type)
                {
                    case GameDataType.Figure: _tcsFigureData.TrySetException(ex); break;
                    case GameDataType.Furni: _tcsFurniData.TrySetException(ex); break;
                    case GameDataType.Product: _tcsProductData.TrySetException(ex); break;
                    case GameDataType.Texts: _tcsExternalTexts.TrySetException(ex); break;
                    default: break;
                }

                MetadataLoadFailed?.Invoke(this, new GameDataEventArgs(type));
            }
        }

        public Task<FigureData> GetFigureDataAsync() => _tcsFigureData.Task;
        public Task<FurniData> GetFurniDataAsync() => _tcsFurniData.Task;
        public Task<ProductData> GetProductDataAsync() => _tcsProductData.Task;
        public Task<ExternalTexts> GetExternalTextsAsync() => _tcsExternalTexts.Task;
    }
}

using System.Net.Http.Json;
using System.Text.Json;
using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Weapon;
using Xunkong.Hoyolab.Wishlog;

namespace Xunkong.ApiClient;

public class XunkongApiClient
{

    private readonly HttpClient _httpClient;

#if !NativeAOT
    private readonly JsonSerializerOptions _jsonOptions;
#endif

    private const string BaseUrl = "https://api.xunkong.cc";

    public const string ApiVersion = "v0.1";

#if NativeAOT

    public XunkongApiClient(HttpClient? httpClient = null)
    {
        if (httpClient is null)
        {
            _httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.All });
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "XunkongApiClient/0.1.0");
        }
        else
        {
            _httpClient = httpClient;
        }
    }

#else

    public XunkongApiClient(HttpClient? httpClient = null, JsonSerializerOptions? jsonOptions = null)
    {
        if (httpClient is null)
        {
            _httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.All });
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "XunkongApiClient/0.1.0");
        }
        else
        {
            _httpClient = httpClient;
        }
        if (jsonOptions is null)
        {
            _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }
        else
        {
            _jsonOptions = jsonOptions;
        }
    }

#endif

    #region Common Method


    private async Task<T> CommonGetAsync<T>(string url) where T : class
    {
#if NativeAOT
        var wrapper = await _httpClient.GetFromJsonAsync(url, typeof(ApiBaseWrapper<T>), XunkongApiJsonContext.Default) as ApiBaseWrapper<T>;
#else
        var wrapper = await _httpClient.GetFromJsonAsync<ApiBaseWrapper<T>>(url, _jsonOptions);
#endif
        if (wrapper is null)
        {
            throw new XunkongApiException(-2, "Response body is null.");
        }
        if (wrapper.Code != 0)
        {
            throw new XunkongApiException(wrapper.Code, wrapper.Message);
        }
        if (wrapper.Data is null)
        {
            throw new XunkongApiException(-2, "Response data is null.");
        }
        return wrapper.Data;
    }



    private async Task<T> CommonPostAsync<T>(string url, object value) where T : class
    {
        var response = await _httpClient.PostAsJsonAsync(url, value);
        response.EnsureSuccessStatusCode();
#if NativeAOT
        var wrapper = await JsonSerializer.DeserializeAsync(await response.Content.ReadAsStreamAsync(), typeof(ApiBaseWrapper<T>), XunkongApiJsonContext.Default) as ApiBaseWrapper<T>;
#else
        var wrapper = await JsonSerializer.DeserializeAsync<ApiBaseWrapper<T>>(await response.Content.ReadAsStreamAsync(), _jsonOptions);
#endif
        if (wrapper is null)
        {
            throw new XunkongApiException(-2, "Response body is null.");
        }
        if (wrapper.Code != 0)
        {
            throw new XunkongApiException(wrapper.Code, wrapper.Message);
        }
        if (wrapper.Data is null)
        {
            throw new XunkongApiException(-2, "Response data is null.");
        }
        return wrapper.Data;
    }



    private async Task<T> CommonSendAsync<T>(HttpRequestMessage request) where T : class
    {
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
#if NativeAOT
        var wrapper = await JsonSerializer.DeserializeAsync(await response.Content.ReadAsStreamAsync(), typeof(ApiBaseWrapper<T>), XunkongApiJsonContext.Default) as ApiBaseWrapper<T>;
#else
        var wrapper = await JsonSerializer.DeserializeAsync<ApiBaseWrapper<T>>(await response.Content.ReadAsStreamAsync(), _jsonOptions);
#endif
        if (wrapper is null)
        {
            throw new XunkongApiException(-2, "Response body is null.");
        }
        if (wrapper.Code != 0)
        {
            throw new XunkongApiException(wrapper.Code, wrapper.Message);
        }
        if (wrapper.Data is null)
        {
            throw new XunkongApiException(-2, "Response data is null.");
        }
        return wrapper.Data;
    }


    #endregion



    #region Desktop Version



    public async Task<DesktopUpdateVersion> CheckDesktopUpdateAsync(ChannelType channel)
    {
        var url = $"{BaseUrl}/{ApiVersion}/desktop/checkupdate?channel={channel}";
        return await CommonGetAsync<DesktopUpdateVersion>(url);
    }


#if NativeAOT

    public async Task<NotificationWrapper<NotificationModelBase>> GetNotificationsAsync(ChannelType channel, Version version, int lastId = 0)
    {
        var url = $"{BaseUrl}/{ApiVersion}/desktop/notifications?channel={channel}&version={version}&lastId={lastId}";
        return await CommonGetAsync<NotificationWrapper<NotificationModelBase>>(url);
    }

#else

    public async Task<NotificationWrapper<T>> GetNotificationsAsync<T>(ChannelType channel, Version version, int lastId = 0) where T : NotificationModelBase
    {
        var url = $"{BaseUrl}/{ApiVersion}/desktop/notifications?channel={channel}&version={version}&lastId={lastId}";
        return await CommonGetAsync<NotificationWrapper<T>>(url);
    }

#endif

    #endregion



    #region Wishlog Cloud Backup


    public async Task<WishlogBackupResult> GetWishlogLastItemFromCloudAsync(WishlogBackupRequest request)
    {
        var url = $"{BaseUrl}/{ApiVersion}/wishlog/last";
        return await CommonPostAsync<WishlogBackupResult>(url, request);
    }


    public async Task<WishlogBackupResult> GetWishlogListFromCloudAsync(WishlogBackupRequest request)
    {
        var url = $"{BaseUrl}/{ApiVersion}/wishlog/get";
        return await CommonPostAsync<WishlogBackupResult>(url, request);
    }


    public async Task<WishlogBackupResult> PutWishlogListToCloudAsync(WishlogBackupRequest request)
    {
        var url = $"{BaseUrl}/{ApiVersion}/wishlog/put";
        return await CommonPostAsync<WishlogBackupResult>(url, request);
    }


    public async Task<WishlogBackupResult> DeleteWishlogInCloudAsync(WishlogBackupRequest request)
    {
        var url = $"{BaseUrl}/{ApiVersion}/wishlog/delete";
        return await CommonPostAsync<WishlogBackupResult>(url, request);
    }


    #endregion



    #region Genshin Data



    public async Task<IEnumerable<CharacterInfo>> GetCharacterInfosAsync()
    {
        var url = $"{BaseUrl}/{ApiVersion}/genshindata/character";
        var result = await CommonGetAsync<GenshinDataWrapper<CharacterInfo>>(url);
        return result.List;
    }


    public async Task<IEnumerable<WeaponInfo>> GetWeaponInfosAsync()
    {
        var url = $"{BaseUrl}/{ApiVersion}/genshindata/weapon";
        var result = await CommonGetAsync<GenshinDataWrapper<WeaponInfo>>(url);
        return result.List;
    }


    public async Task<IEnumerable<WishEventInfo>> GetWishEventInfosAsync()
    {
        var url = $"{BaseUrl}/{ApiVersion}/genshindata/wishevent";
        var result = await CommonGetAsync<GenshinDataWrapper<WishEventInfo>>(url);
        return result.List;
    }


    #endregion



    #region Genshin Wallpaper


    public async Task<WallpaperInfo> GetRecommendWallpaperAsync()
    {
        var url = $"{BaseUrl}/{ApiVersion}/wallpaper/recommend";
        return await CommonGetAsync<WallpaperInfo>(url);
    }


    public async Task<WallpaperInfo> GetRandomWallpaperAsync()
    {
        var url = $"{BaseUrl}/{ApiVersion}/wallpaper/random";
        return await CommonGetAsync<WallpaperInfo>(url);
    }


    public async Task<WallpaperInfo> GetNextWallpaperAsync(int lastId = 0)
    {
        var url = $"{BaseUrl}/{ApiVersion}/wallpaper/next?lastId={lastId}";
        return await CommonGetAsync<WallpaperInfo>(url);
    }



    #endregion

}

using System.Net.Http.Json;
using Xunkong.ApiClient.GenshinData;
using Xunkong.ApiClient.Xunkong;
using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Material;
using Xunkong.GenshinData.Weapon;
using Xunkong.Hoyolab.Wishlog;

namespace Xunkong.ApiClient;

public class XunkongApiClient
{

    private readonly HttpClient _httpClient;

    private const string BaseUrl = "https://api.xunkong.cc";

    public const string ApiVersion = "v0.1";



    public XunkongApiClient(HttpClient? httpClient = null)
    {
        if (httpClient is null)
        {
            _httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.All });
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "XunkongApiClient/1.0.0");
        }
        else
        {
            _httpClient = httpClient;
        }
    }


    #region Common Method


    private async Task<T> CommonGetAsync<T>(string url) where T : class
    {
        var wrapper = await _httpClient.GetFromJsonAsync<ApiBaseWrapper<T>>(url);
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
        var wrapper = await response.Content.ReadFromJsonAsync<ApiBaseWrapper<T>>();
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
        var wrapper = await response.Content.ReadFromJsonAsync<ApiBaseWrapper<T>>();
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



    public async Task<List<InfoBarContent>> GetInfoBarContentListAsync(ChannelType channel, Version version)
    {
        var url = $"{BaseUrl}/{ApiVersion}/desktop/infobar?channel={channel}&version={version}";
        var wrapper = await CommonGetAsync<ListWrapper<InfoBarContent>>(url);
        return wrapper.List;
    }



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



    public async Task<AllGenshinData> GetAllGenshinDataAsync()
    {
        var url = $"{BaseUrl}/{ApiVersion}/genshindata/all";
        return await CommonGetAsync<AllGenshinData>(url);
    }


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


    public async Task<IEnumerable<NameCard>> GetNameCardsAsync()
    {
        var url = $"{BaseUrl}/{ApiVersion}/genshindata/namecard";
        var result = await CommonGetAsync<GenshinDataWrapper<NameCard>>(url);
        return result.List;
    }


    public async Task<Achievement> GetAchievementsAsync()
    {
        var url = $"{BaseUrl}/{ApiVersion}/genshindata/achievement";
        return await CommonGetAsync<Achievement>(url);
    }



    #endregion



    #region Genshin Wallpaper


    public async Task<WallpaperInfo> GetWallpaperByIdAsync(int id)
    {
        var url = $"{BaseUrl}/{ApiVersion}/wallpaper/{id}";
        return await CommonGetAsync<WallpaperInfo>(url);
    }

    public async Task<WallpaperInfo> GetRecommendWallpaperAsync()
    {
        var url = $"{BaseUrl}/{ApiVersion}/wallpaper/recommend";
        return await CommonGetAsync<WallpaperInfo>(url);
    }


    public async Task<WallpaperInfo> GetRandomWallpaperAsync(int maxage = 0)
    {
        var url = $"{BaseUrl}/{ApiVersion}/wallpaper/random?max-age={maxage}";
        return await CommonGetAsync<WallpaperInfo>(url);
    }


    public async Task<WallpaperInfo> GetNextWallpaperAsync(int lastId = 0)
    {
        var url = $"{BaseUrl}/{ApiVersion}/wallpaper/next?lastId={lastId}";
        return await CommonGetAsync<WallpaperInfo>(url);
    }



    public async Task<List<WallpaperInfo>> GetWallpaperListAsync(int size)
    {
        var url = $"{BaseUrl}/{ApiVersion}/wallpaper/list?size={size}";
        var wrapper = await CommonGetAsync<ListWrapper<WallpaperInfo>>(url);
        return wrapper.List;
    }




    #endregion

}

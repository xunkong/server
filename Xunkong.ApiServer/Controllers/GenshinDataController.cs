using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Weapon;

namespace Xunkong.ApiServer.Controllers;

[ApiController]
[ApiVersion("0.1")]
[Route("v{version:ApiVersion}/[controller]")]
[ServiceFilter(typeof(BaseRecordResultFilter))]
[ResponseCache(Duration = 3600)]
public class GenshinDataController : ControllerBase
{
    private readonly ILogger<GenshinMetadataController> _logger;

    private readonly XunkongDbContext _dbContext;

    private readonly DbConnectionFactory _dbFactory;


    public GenshinDataController(ILogger<GenshinMetadataController> logger, XunkongDbContext dbContext, DbConnectionFactory dbFactory)
    {
        _logger = logger;
        _dbContext = dbContext;
        _dbFactory = dbFactory;
    }


    private string ParseLanguage(string? language)
    {
        return language?.ToLower() switch
        {
            "de-de" or "de" => "de-de",
            "en-us" or "en" => "en-us",
            "es-es" or "es" => "es-es",
            "fr-fr" or "fr" => "fr-fr",
            "id-id" or "id" => "id-id",
            "ja-jp" or "ja" => "ja-jp",
            "ko-kr" or "ko" => "ko-kr",
            "pt-pt" or "pt" => "pt-pt",
            "ru-ru" or "ru" => "ru-ru",
            "th-th" or "th" => "th-th",
            "vi-vn" or "vi" => "vi-vn",
            "zh-tw" or "cht" => "zh-tw",
            _ => "zh-cn",
        };
    }


    [HttpGet("character")]
    public async Task<object> GetCharacterInfos([FromQuery] string? language = null)
    {
        var lang = ParseLanguage(language);
        var characters = await _dbContext.CharacterInfos.AsNoTracking().Where(x => x.Enable).Include(x => x.Talents).Include(x => x.Constellations).ToListAsync();
        if (lang != "zh-cn")
        {
            await ConverterToAnotherLanguage(characters, lang);
        }
        var list = characters.Adapt<List<CharacterInfo>>();
        return new { Language = lang, list.Count, List = list };
    }


    [HttpGet("weapon")]
    public async Task<object> GetWeaponInfos([FromQuery] string? language = null)
    {
        var lang = ParseLanguage(language);
        var weapons = await _dbContext.WeaponInfos.AsNoTracking().Where(x => x.Enable).Include(x => x.Skills).ToListAsync();
        if (lang != "zh-cn")
        {
            await ConverterToAnotherLanguage(weapons, lang);
        }
        var list = weapons.Adapt<List<WeaponInfo>>();
        return new { Language = lang, list.Count, List = list };
    }


    [HttpGet("wishevent")]
    public async Task<object> GetWishEventInfos()
    {
        var list = await _dbContext.WishEventInfos.AsNoTracking().ToListAsync();
        return new { Language = "zh-cn", list.Count, List = list };
    }


    /// <summary>
    /// 保持兼容性，返回空数组
    /// </summary>
    /// <returns></returns>
    [HttpGet("i18n")]
    public object GetI18nModelsAsync()
    {
        return new { Language = "", Count = 0, List = new List<int>() };
    }


    private async Task ConverterToAnotherLanguage(IEnumerable<CharacterInfoModel> characterInfoModels, string lang)
    {
        using var dapper = _dbFactory.CreateDbConnection();
        var dy = await dapper.QueryAsync<(long Id, string? Text)>($"SELECT Id,{lang.Replace('-', '_')} FROM textmaps WHERE Type & 0x0F;");
        var dic = dy.ToDictionary(x => x.Id, x => x.Text);
        foreach (var item in characterInfoModels)
        {
            item.Name = dic[item.NameTextMapHash];
            item.Title = dic[item.TitleTextMapHash];
            item.Description = dic[item.DescTextMapHash];
            item.Affiliation = dic[item.AffiliationTextMapHash];
            item.ConstllationName = dic[item.ConstllationTextMapHash];
            item.CvChinese = dic[item.CvChineseTextMapHash];
            item.CvJapanese = dic[item.CvJapaneseTextMapHash];
            item.CvEnglish = dic[item.CvEnglishTextMapHash];
            item.CvKorean = dic[item.CvKoreanTextMapHash];
        }
        foreach (var item in characterInfoModels.SelectMany(x => x.Talents ?? new()))
        {
            item.Name = dic[item.NameTextMapHash];
            item.Description = dic[item.DescTextMapHash];
        }
        foreach (var item in characterInfoModels.SelectMany(x => x.Constellations ?? new()))
        {
            item.Name = dic[item.NameTextMapHash];
            item.Description = dic[item.DescTextMapHash];
        }
    }


    private async Task ConverterToAnotherLanguage(IEnumerable<WeaponInfoModel> weaponInfoModels, string lang)
    {
        using var dapper = _dbFactory.CreateDbConnection();
        lang = lang.Replace('-', '_');
        var dy = await dapper.QueryAsync<(long Id, string? Text)>($"SELECT Id,{lang} FROM textmaps WHERE Type & 0xF0;");
        var dic = dy.ToDictionary(x => x.Id, x => x.Text);
        foreach (var item in weaponInfoModels)
        {
            item.Name = dic[item.NameTextMapHash];
            item.Description = dic[item.DescTextMapHash];
        }
        foreach (var item in weaponInfoModels.SelectMany(x => x.Skills ?? new()))
        {
            item.Name = dic[item.NameTextMapHash];
            item.Description = dic[item.DescTextMapHash];
        }
        var dy2 = await dapper.QueryAsync<(int Id, string? Text)>($"SELECT r.Id,t.{lang} FROM readables r LEFT JOIN readabletextmaps t ON r.ContentId=t.Id;");
        var dic2 = dy2.ToDictionary(x => x.Id, x => x.Text);
        foreach (var item in weaponInfoModels)
        {
            item.Story = dic2[item.StoryId];
        }
    }



    [HttpGet("raw/character")]
    public async Task<object> GetRawCharacterAsync()
    {
        var list = await _dbContext.CharacterInfos.AsNoTracking().Where(x => x.Enable).Include(x => x.Talents).Include(x => x.Constellations).ToListAsync();
        return new { Language = "", list.Count, List = list };
    }



    [HttpGet("raw/weapon")]
    public async Task<object> GetRawWeaponAsync()
    {
        var list = await _dbContext.WeaponInfos.AsNoTracking().Where(x => x.Enable).Include(x => x.Skills).ToListAsync();
        return new { Language = "", list.Count, List = list };
    }



    [HttpGet("raw/textmap")]
    public async Task<object> GetRawTextMapAsync()
    {
        var list = await _dbContext.TextMaps.AsNoTracking().ToListAsync();
        return new { Language = "", list.Count, List = list };
    }

}

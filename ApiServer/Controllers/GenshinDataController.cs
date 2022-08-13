using Xunkong.ApiClient.GenshinData;
using Xunkong.GenshinData.Achievement;
using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Material;
using Xunkong.GenshinData.Weapon;

namespace Xunkong.ApiServer.Controllers;

[ApiController]
[ApiVersion("0.1")]
[Route("v{version:ApiVersion}/[controller]")]
[ServiceFilter(typeof(BaseRecordResultFilter))]
#if !DEBUG
[ResponseCache(Duration = 3600)]
#endif
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


    [HttpGet("all")]
    public async Task<object> GetAllGenshinDataAsync()
    {
        var characters = await _dbContext.CharacterInfos.AsNoTracking().Where(x => x.Enable).Include(x => x.Talents).Include(x => x.Constellations).ToListAsync();
        var weapons = await _dbContext.WeaponInfos.AsNoTracking().Where(x => x.Enable).Include(x => x.Skills).ToListAsync();
        var events = await _dbContext.WishEventInfos.AsNoTracking().ToListAsync();
        return new AllGenshinData { Characters = characters.Adapt<List<CharacterInfo>>(), Weapons = weapons.Adapt<List<WeaponInfo>>(), WishEvents = events, Achievement = await GetAchievementAsync() };
    }



    [HttpGet("character")]
    public async Task<object> GetCharacterInfos()
    {
        var characters = await _dbContext.CharacterInfos.AsNoTracking().Where(x => x.Enable).Include(x => x.Talents).Include(x => x.Constellations).ToListAsync();
        var list = characters.Adapt<List<CharacterInfo>>();
        return new { Count = list.Count, List = list };
    }


    [HttpGet("weapon")]
    public async Task<object> GetWeaponInfos()
    {
        var weapons = await _dbContext.WeaponInfos.AsNoTracking().Where(x => x.Enable).Include(x => x.Skills).ToListAsync();
        var list = weapons.Adapt<List<WeaponInfo>>();
        return new { Count = list.Count, List = list };
    }


    [HttpGet("wishevent")]
    public async Task<object> GetWishEventInfos()
    {
        var list = await _dbContext.WishEventInfos.AsNoTracking().ToListAsync();
        return new { Count = list.Count, List = list };
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



    [HttpGet("namecard")]
    public async Task<object> GetNameCardsAsync()
    {
        using var dapper = _dbFactory.CreateDbConnection();
        var list = await dapper.QueryAsync<NameCard>($"SELECT Id, Name, Description, Icon, ItemType, MaterialType, TypeDescription, RankLevel, StackLimit, `Rank`, GalleryBackground, ProfileImage FROM info_material WHERE Enable AND MaterialType='{MaterialType.NameCard}';");
        return new { Count = list.Count(), List = list };
    }




    [HttpGet("achievement")]
    public async Task<Achievement> GetAchievementAsync()
    {
        var goals = await _dbContext.Set<AchievementGoal>().AsNoTracking().ToListAsync();
        var items = await _dbContext.Set<AchievementItem>().FromSqlRaw("SELECT * FROM info_achievement_item WHERE Enable;").AsNoTracking().ToListAsync();
        return new Achievement { Goals = goals, Items = items };
    }


}

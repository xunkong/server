using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Weapon;

namespace Xunkong.ApiServer.Controllers;

[ApiController]
[ApiVersion("0.1")]
[Route("v{version:ApiVersion}/Genshin/Metadata")]
[ServiceFilter(typeof(BaseRecordResultFilter))]
[ResponseCache(Duration = 3600)]
public class GenshinMetadataController : ControllerBase
{

    private readonly ILogger<GenshinMetadataController> _logger;

    private readonly XunkongDbContext _dbContext;


    public GenshinMetadataController(ILogger<GenshinMetadataController> logger, XunkongDbContext dbContext, DbConnectionFactory dbFactory)
    {
        _logger = logger;
        _dbContext = dbContext;
    }



    [HttpGet("character")]
    public async Task<object> GetCharacterInfos()
    {
        var characters = await _dbContext.CharacterInfos.AsNoTracking().Where(x => x.Enable).Include(x => x.Talents).Include(x => x.Constellations).ToListAsync();
        var list = characters.Adapt<List<CharacterInfo>>();
        return new { Language = "zh-cn", list.Count, List = list };
    }


    [HttpGet("weapon")]
    public async Task<object> GetWeaponInfos()
    {
        var weapons = await _dbContext.WeaponInfos.AsNoTracking().Where(x => x.Enable).Include(x => x.Skills).ToListAsync();
        var list = weapons.Adapt<List<WeaponInfo>>();
        return new { Language = "zh-cn", list.Count, List = list };
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


}

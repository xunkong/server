namespace Xunkong.ApiServer.Controllers;

[ApiController]
[ApiVersion("0.1")]
[Route("v{version:ApiVersion}/[controller]")]
[ServiceFilter(typeof(BaseRecordResultFilter))]
public class WallpaperController : Controller
{


    private readonly ILogger<GenshinWallpaperController> _logger;

    private readonly XunkongDbContext _dbContext;



    public WallpaperController(ILogger<GenshinWallpaperController> logger, XunkongDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }




    [HttpGet("{id}")]
    [ResponseCache(Duration = 2592000)]
    public async Task<WallpaperInfo> GetWallpaperInfoByIdAsync([FromRoute] int id)
    {
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Id == id).SingleOrDefaultAsync();
        if (info is not null)
        {
            return info;
        }
        else
        {
            throw new XunkongApiServerException(ErrorCode.InternalException, "Wallpaper not found.");
        }
    }



    [NoWrapper]
    [HttpGet("{id}/redirect")]
    [ResponseCache(Duration = 2592000)]
    public async Task<IActionResult> RedirectToWallpaperImageByIdAsync([FromRoute] int id)
    {
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Id == id).SingleOrDefaultAsync();
        if (info is not null)
        {
            return Redirect(info.Url);
        }
        else
        {
            return NotFound("Wallpaper not found.");
        }
    }



    [HttpGet("recommend")]
    [ResponseCache(Duration = 3600)]
    public async Task<WallpaperInfo> GetRecommendWallpaperInfoAsync()
    {
        var count = await _dbContext.WallpaperInfos.CountAsync();
        var id = Random.Shared.Next(count) + 1;
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Id == id).FirstOrDefaultAsync();
        if (info is null)
        {
            id = Random.Shared.Next(count);
            info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Id == id).FirstOrDefaultAsync();
        }
        return info!;
    }


    /// <summary>
    /// 不要删除
    /// </summary>
    /// <returns></returns>
    [NoWrapper]
    [HttpGet("recommend/redirect")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> RedirectToRecommendWallpaperImageAsync()
    {
        var count = await _dbContext.WallpaperInfos.CountAsync();
        var id = Random.Shared.Next(count) + 1;
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable && x.Id == id).FirstOrDefaultAsync();
        if (info is null)
        {
            id = Random.Shared.Next(count);
            info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable && x.Id == id).FirstOrDefaultAsync();
        }
        return Redirect(info!.Url);
    }



    [HttpGet("random")]
    public async Task<WallpaperInfo> GetRandomWallpaperInfoAsync([FromQuery(Name = "max-age")] int maxage)
    {
        var count = await _dbContext.WallpaperInfos.CountAsync();
        var id = Random.Shared.Next(count) + 1;
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable && x.Id == id).FirstOrDefaultAsync();
        if (info is null)
        {
            id = Random.Shared.Next(count);
            info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable && x.Id == id).FirstOrDefaultAsync();
        }
        maxage = Math.Clamp(maxage, 5, 3600 * 24);
        Response.Headers["Cache-Control"] = $"public,max-age={maxage}";
        return info!;
    }


    /// <summary>
    /// 不要删除
    /// </summary>
    /// <returns></returns>
    [NoWrapper]
    [HttpGet("random/redirect")]
    public async Task<IActionResult> RedirectToRandomWallpaperImageAsync([FromQuery(Name = "max-age")] int maxage)
    {
        var count = await _dbContext.WallpaperInfos.CountAsync();
        var id = Random.Shared.Next(count) + 1;
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable && x.Id == id).FirstOrDefaultAsync();
        if (info is null)
        {
            id = Random.Shared.Next(count);
            info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable && x.Id == id).FirstOrDefaultAsync();
        }
        maxage = Math.Clamp(maxage, 5, 3600 * 24);
        Response.Headers["Cache-Control"] = $"public,max-age={maxage}";
        return Redirect(info!.Url);
    }



    [HttpGet("next")]
    [ResponseCache(Duration = 86400)]
    public async Task<WallpaperInfo> GetNextWallpaperInfoAsync([FromQuery] int lastId)
    {
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).Where(x => x.Id > lastId).FirstOrDefaultAsync();
        if (info == null)
        {
            info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).FirstOrDefaultAsync();
        }
        return info!;
    }



    [HttpGet("list")]
    [ResponseCache(Duration = 86400)]
    public async Task<WallpaperInfoListWrapper> GetWallpaperInfosAsync([FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var count = await _dbContext.WallpaperInfos.Where(x => x.Enable).CountAsync();
        var totalPage = count / size + 1;
        page = Math.Clamp(page, 1, totalPage);
        var infos = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).Skip(size * page - size).Take(size).ToListAsync();
        return new WallpaperInfoListWrapper(page, totalPage, infos.Count, infos);
    }



}

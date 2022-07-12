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
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable && x.Id == id).SingleOrDefaultAsync();
        if (info is not null)
        {
            return RedirectToImage(info.FileName!);
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
        var count = await _dbContext.WallpaperInfos.Where(x => x.Recommend).CountAsync();
        var skip = Random.Shared.Next(count);
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Recommend).Skip(skip).FirstOrDefaultAsync();
        if (info is null)
        {
            count = await _dbContext.WallpaperInfos.Where(x => x.Enable).CountAsync();
            skip = Random.Shared.Next(count);
            info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).Skip(skip).FirstOrDefaultAsync();
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
        var count = await _dbContext.WallpaperInfos.Where(x => x.Recommend).CountAsync();
        var skip = Random.Shared.Next(count);
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Recommend).Skip(skip).FirstOrDefaultAsync();
        if (info is null)
        {
            count = await _dbContext.WallpaperInfos.Where(x => x.Enable).CountAsync();
            skip = Random.Shared.Next(count);
            info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).Skip(skip).FirstOrDefaultAsync();
        }
        return RedirectToImage(info!.FileName!);
    }



    [HttpGet("random")]
    public async Task<WallpaperInfo> GetRandomWallpaperInfoAsync([FromQuery(Name = "max-age")] int maxage)
    {
        var count = await _dbContext.WallpaperInfos.Where(x => x.Enable).CountAsync();
        var skip = Random.Shared.Next(count);
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).Skip(skip).FirstOrDefaultAsync();
        maxage = Math.Clamp(maxage, 5, 3600 * 24);
        Response.Headers["Cache-Control"] = $"max-age={maxage}";
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
        var count = await _dbContext.WallpaperInfos.Where(x => x.Enable).CountAsync();
        var skip = Random.Shared.Next(count);
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).Skip(skip).FirstOrDefaultAsync();
        maxage = Math.Clamp(maxage, 5, 3600 * 24);
        Response.Headers["Cache-Control"] = $"max-age={maxage}";
        return RedirectToImage(info!.FileName!);
    }



    [HttpGet("next")]
    [ResponseCache(Duration = 86400)]
    public async Task<WallpaperInfo> GetNextWallpaperInfoAsync([FromQuery] int lastId)
    {
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable && x.Id > lastId).FirstOrDefaultAsync();
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




    private IActionResult RedirectToImage(string fileName, string? style = null)
    {
        var url = $"https://file.xunkong.cc/wallpaper/{Uri.EscapeDataString(fileName)}{style}";
        return Redirect(url);
    }


}

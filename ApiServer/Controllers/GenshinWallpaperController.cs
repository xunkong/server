namespace Xunkong.ApiServer.Controllers;

[ApiController]
[ApiVersion("0.1")]
[Route("v{version:ApiVersion}/Genshin/Wallpaper")]
[ServiceFilter(typeof(BaseRecordResultFilter))]
public class GenshinWallpaperController : Controller
{


    private readonly ILogger<GenshinWallpaperController> _logger;

    private readonly XunkongDbContext _dbContext;

    public GenshinWallpaperController(ILogger<GenshinWallpaperController> logger, XunkongDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }



    [HttpGet("random")]
    [ResponseCache(NoStore = true)]
    public async Task<WallpaperInfo> GetRandomWallpaperAsJsonResultAsync(int excludeId = 0)
    {
        var count = await _dbContext.WallpaperInfos.CountAsync();
        var id = Random.Shared.Next(count) + 1;
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable && x.Id == id).FirstOrDefaultAsync();
        if (info is null)
        {
            id = Random.Shared.Next(count);
            info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable && x.Id == id).FirstOrDefaultAsync();
        }
        return info!;
    }



    [HttpGet("next")]
    [ResponseCache(NoStore = true)]
    public async Task<WallpaperInfo> GetNextWallpaperAsJsonResultAsync(int excludeId = 0)
    {
        var info = await _dbContext.WallpaperInfos.Where(x => x.Enable && x.Id > excludeId).OrderBy(x => x.Id).FirstOrDefaultAsync();
        if (info == null)
        {
            info = await _dbContext.WallpaperInfos.Where(x => x.Enable).OrderBy(x => x.Id).FirstOrDefaultAsync();
        }
        return info!;
    }



    [NoWrapper]
    [HttpGet("redirect/recommend")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> RedirectRecommendWallpaperAsync()
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




    [NoWrapper]
    [HttpGet("redirect/random")]
    [ResponseCache(NoStore = true)]
    public async Task<IActionResult> RedirectRandomWallpaperAsync()
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



}

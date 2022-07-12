using System.Web;

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
        WallpaperInfo? info = null;
        if (excludeId == 0)
        {
            var count = await _dbContext.WallpaperInfos.Where(x => x.Recommend).CountAsync();
            var index = Random.Shared.Next(count);
            info = await _dbContext.WallpaperInfos.Where(x => x.Recommend).Skip(index).FirstOrDefaultAsync();
        }
        if (info == null)
        {
            var count = await _dbContext.WallpaperInfos.Where(x => x.Enable).CountAsync();
            var index = Random.Shared.Next(count - 1);
            info = await _dbContext.WallpaperInfos.Where(x => x.Enable && x.Id != excludeId).Skip(index).FirstOrDefaultAsync();
        }
        if (info == null)
        {
            info = await _dbContext.WallpaperInfos.Where(x => x.Enable).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
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
        var count = await _dbContext.WallpaperInfos.Where(x => x.Recommend).CountAsync();
        var index = Random.Shared.Next(count);
        var info = await _dbContext.WallpaperInfos.Where(x => x.Recommend).Skip(index).FirstOrDefaultAsync();
        if (info is null)
        {
            count = await _dbContext.WallpaperInfos.Where(x => x.Enable).CountAsync();
            index = Random.Shared.Next(count);
            info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).Skip(index).FirstOrDefaultAsync();
        }
        var url = $"https://file.xunkong.cc/wallpaper/{HttpUtility.UrlEncode(info!.FileName)}";
        return Redirect(url);
    }




    [NoWrapper]
    [HttpGet("redirect/random")]
    [ResponseCache(NoStore = true)]
    public async Task<IActionResult> RedirectRandomWallpaperAsync()
    {
        var count = await _dbContext.WallpaperInfos.Where(x => x.Enable).CountAsync();
        var index = Random.Shared.Next(count);
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).Skip(index).FirstOrDefaultAsync();
        var url = $"https://file.xunkong.cc/wallpaper/{HttpUtility.UrlEncode(info!.FileName)}";
        return Redirect(url);
    }



}

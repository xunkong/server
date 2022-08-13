using System.Collections.Concurrent;

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
        return await RandomNextAsync();
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
        var info = await RandomNextAsync();
        return Redirect(info.Url);
    }



    [HttpGet("random")]
    public async Task<WallpaperInfo> GetRandomWallpaperInfoAsync([FromQuery(Name = "max-age")] int maxage)
    {
        var info = await RandomNextAsync();
        maxage = Math.Clamp(maxage, 5, 3600 * 24);
        Response.Headers["Cache-Control"] = $"public,max-age={maxage}";
        if (info is null)
        {
            throw new FileNotFoundException();
        }
        return info;
    }


    /// <summary>
    /// 不要删除
    /// </summary>
    /// <returns></returns>
    [NoWrapper]
    [HttpGet("random/redirect")]
    public async Task<IActionResult> RedirectToRandomWallpaperImageAsync([FromQuery(Name = "max-age")] int maxage)
    {
        var info = await RandomNextAsync();
        maxage = Math.Clamp(maxage, 5, 3600 * 24);
        Response.Headers["Cache-Control"] = $"public,max-age={maxage}";
        return Redirect(info.Url);
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
    [ResponseCache(Duration = 5)]
    public async Task<object> GetWallpaperInfosAsync([FromQuery] int size = 20)
    {
        size = Math.Clamp(size, 10, 40);
        var list = new List<WallpaperInfo>(size);
        for (int i = 0; i < size; i++)
        {
            list.Add(await RandomNextAsync());
        }
        return new { Count = size, List = list };
    }



    private static ConcurrentQueue<WallpaperInfo> _randomQueue = new();

    private async ValueTask<WallpaperInfo> RandomNextAsync()
    {
        if (_randomQueue.IsEmpty)
        {
            var infos = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).ToArrayAsync();
            // Fisher–Yates shuffle
            for (int i = infos.Length - 1; i > 0; i--)
            {
                int j = Random.Shared.Next(i + 1);
                (infos[i], infos[j]) = (infos[j], infos[i]);
            }
            foreach (var item in infos)
            {
                _randomQueue.Enqueue(item);
            }
        }
        if (_randomQueue.TryDequeue(out var info))
        {
            return info;
        }
        else
        {
            return null!;
        }
    }



}

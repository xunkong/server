using System.Collections.Concurrent;
using Xunkong.ApiClient.Xunkong;

namespace Xunkong.ApiServer.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("v{version:ApiVersion}/[controller]")]
[ServiceFilter(typeof(BaseRecordResultFilter))]
public class WallpaperController : Controller
{


    private readonly ILogger<WallpaperController> _logger;

    private readonly XunkongDbContext _dbContext;

    private readonly DbConnectionFactory _factory;


    public WallpaperController(ILogger<WallpaperController> logger, XunkongDbContext dbContext, DbConnectionFactory factory)
    {
        _logger = logger;
        _dbContext = dbContext;
        _factory = factory;
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
    [ResponseCache(Duration = 5)]
    public async Task<WallpaperInfo> GetRandomWallpaperInfoAsync()
    {
        var info = await RandomNextAsync();
        _logger.LogInformation("Wallpaper:\nId: {Id}\nTitle: {Title}\nUrl: {Url}", info.Id, info.Title, info.Url);
        return info;
    }


    /// <summary>
    /// 不要删除
    /// </summary>
    /// <returns></returns>
    [NoWrapper]
    [HttpGet("random/redirect")]
    [ResponseCache(Duration = 5)]
    public async Task<IActionResult> RedirectToRandomWallpaperImageAsync()
    {
        var info = await RandomNextAsync();
        return Redirect(info.Url);
    }



    [HttpGet("next")]
    [ResponseCache(Duration = 2592000)]
    public async Task<WallpaperInfo> GetNextWallpaperInfoAsync([FromQuery] int lastId)
    {
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).Where(x => x.Id > lastId).OrderBy(x => x.Id).FirstOrDefaultAsync();
        if (info == null)
        {
            info = await RandomNextAsync();
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



    [HttpPost("rating")]
    [ResponseCache(NoStore = true)]
    public async Task RatingWallpaperAsync([FromHeader(Name = "X-Device-Id")] string deviceId, [FromBody] List<WallpaperRating> ratings)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new XunkongApiException(-1, "DeviceId is wrong.");
        }
        foreach (var rating in ratings)
        {
            if (rating.DeviceId != deviceId)
            {
                throw new XunkongApiException(-1, "DeviceId is wrong.");
            }
            rating.Rating = Math.Clamp(rating.Rating, -1, 5);
        }
        using var dapper = _factory.CreateDbConnection();
        await dapper.OpenAsync();
        using var t = await dapper.BeginTransactionAsync();
        await dapper.ExecuteAsync("""
            INSERT INTO wallpaper_rating (WallpaperId, DeviceId, Rating, Time)
            VALUES (@WallpaperId, @DeviceId, @Rating, @Time)
            ON DUPLICATE KEY UPDATE Rating=@Rating, Time=@Time;
            """, ratings, t);
        await t.CommitAsync();
    }



    private static ConcurrentQueue<WallpaperInfo> _randomQueue = new();

    private async ValueTask<WallpaperInfo> RandomNextAsync()
    {
        if (_randomQueue.IsEmpty)
        {
            _logger.LogInformation("Wallpaper queue is empty, start get new randoms.");
            var infos = await _dbContext.WallpaperInfos.FromSqlRaw("SELECT * FROM wallpapers WHERE Enable ORDER BY RAND() LIMIT 720;").AsNoTracking().ToArrayAsync();
            foreach (var item in infos)
            {
                _randomQueue.Enqueue(item);
            }
            _logger.LogInformation("Enqueue finished.");
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

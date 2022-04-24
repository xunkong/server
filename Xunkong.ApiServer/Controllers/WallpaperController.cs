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
    [ResponseCache(Duration = 3600)]
    public async Task<WallpaperInfo> GetRandomWallpaperInfoAsync()
    {
        var count = await _dbContext.WallpaperInfos.Where(x => x.Enable).CountAsync();
        var skip = Random.Shared.Next(count);
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).Skip(skip).FirstOrDefaultAsync();
        return info!;
    }


    [NoWrapper]
    [HttpGet("random/redirect")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> RedirectToRandomWallpaperImageAsync()
    {
        var count = await _dbContext.WallpaperInfos.Where(x => x.Enable).CountAsync();
        var skip = Random.Shared.Next(count);
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).Skip(skip).FirstOrDefaultAsync();
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
    public async Task<WallpaperInfoListWrapper> GetWallpaperInfosAsync([FromQuery] int page = 1)
    {
        var count = await _dbContext.WallpaperInfos.Where(x => x.Enable).CountAsync();
        var totalPage = count / 20 + 1;
        page = Math.Clamp(page, 1, totalPage);
        var infos = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).Skip(20 * page - 20).Take(20).ToListAsync();
        return new WallpaperInfoListWrapper(page, totalPage, infos.Count, infos);
    }


    [NoWrapper]
    [HttpPost("ChangeRecommend")]
    [ResponseCache(NoStore = true)]
    public async Task<ActionResult<WallpaperInfo>> ChangeRecommendWallpaperAsync([FromQuery] int[] id)
    {
        if (HttpContext.Request.Headers["X-Secret"] != Environment.GetEnvironmentVariable("XSECRET"))
        {
            return BadRequest();
        }
        using var t = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            int rows = 0;
            await _dbContext.Database.ExecuteSqlRawAsync("UPDATE wallpapers SET Recommend=0 WHERE Recommend=1;");
            if (id?.Any() ?? false)
            {
                if (id[0] != 0)
                {
                    rows = await _dbContext.Database.ExecuteSqlRawAsync($"UPDATE wallpapers SET Recommend=1 WHERE Id IN ({string.Join(',', id)});");
                }
            }
            else
            {
                var count = await _dbContext.WallpaperInfos.Where(x => x.Enable).CountAsync();
                var skip = Random.Shared.Next(count);
                var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).Skip(skip).FirstOrDefaultAsync();
                rows = await _dbContext.Database.ExecuteSqlRawAsync($"UPDATE wallpapers SET Recommend=1 WHERE Id = {info?.Id ?? 0};");
            }
            await t.CommitAsync();
            return await GetRecommendWallpaperInfoAsync();
        }
        catch (Exception ex)
        {
            await t.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }

    [NoWrapper]
    [HttpPost("ChangeRecommend/redirect")]
    [ResponseCache(NoStore = true)]
    public async Task<IActionResult> ChangeRecommendWallpaperAndRedirectToImageAsync([FromQuery] int[] id)
    {
        if (HttpContext.Request.Headers["X-Secret"] != Environment.GetEnvironmentVariable("XSECRET"))
        {
            return BadRequest();
        }
        using var t = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            int rows = 0;
            await _dbContext.Database.ExecuteSqlRawAsync("UPDATE wallpapers SET Recommend=0 WHERE Recommend=1;");
            if (id?.Any() ?? false)
            {
                if (id[0] != 0)
                {
                    rows = await _dbContext.Database.ExecuteSqlRawAsync($"UPDATE wallpapers SET Recommend=1 WHERE Id IN ({string.Join(',', id)});");
                }
            }
            else
            {
                var count = await _dbContext.WallpaperInfos.Where(x => x.Enable).CountAsync();
                var skip = Random.Shared.Next(count);
                var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).Skip(skip).FirstOrDefaultAsync();
                rows = await _dbContext.Database.ExecuteSqlRawAsync($"UPDATE wallpapers SET Recommend=1 WHERE Id = {info?.Id ?? 0};");
            }
            await t.CommitAsync();
            var recommend = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).FirstOrDefaultAsync();
            return RedirectToImage(recommend!.Url!, "/thumb");
        }
        catch (Exception ex)
        {
            await t.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }




    private IActionResult RedirectToImage(string fileName, string? style = null)
    {
        var url = $"https://file.xunkong.cc/wallpaper/{Uri.EscapeDataString(fileName)}{style}";
        return Redirect(url);
    }


}

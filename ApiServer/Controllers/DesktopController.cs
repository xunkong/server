using Xunkong.ApiClient.Xunkong;

namespace Xunkong.ApiServer.Controllers;

[ApiController]
[ApiVersion("0.1")]
[Route("v{version:ApiVersion}/[controller]")]
[ServiceFilter(typeof(BaseRecordResultFilter))]
[ResponseCache(Duration = 900)]
public class DesktopController : Controller
{

    private readonly ILogger<DesktopController> _logger;

    private readonly XunkongDbContext _dbContext;

    private readonly DbConnectionFactory _factory;


    public DesktopController(ILogger<DesktopController> logger, XunkongDbContext dbContext, DbConnectionFactory factory)
    {
        _logger = logger;
        _dbContext = dbContext;
        _factory = factory;
    }



    [HttpGet("CheckUpdate")]
    public async Task<DesktopUpdateVersion> CheckUpdateAsync([FromQuery] ChannelType channel)
    {
        var version = await _dbContext.DesktopUpdateVersions.AsNoTracking().Where(x => x.Channel.HasFlag(channel)).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
        if (version is not null)
        {
            return version;
        }
        else
        {
            throw new XunkongApiServerException(ErrorCode.NoContentForVersion);
        }
    }




    [HttpGet("Notifications")]
    public async Task<NotificationWrapper<NotificationServerModel>> GetNotificationsAsync([FromQuery] ChannelType channel, [FromQuery] string? version, [FromQuery] int lastId)
    {
        if (!Version.TryParse(version, out var v))
        {
            throw new XunkongApiServerException(ErrorCode.VersionIsNull);
        }
        var vmin = new Version(0, 0);
        var vmax = new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
        var notifications = await _dbContext.NotificationItems.AsNoTracking()
                                                              .Where(x => x.Platform == PlatformType.Desktop && x.Channel.HasFlag(channel) && x.Enable)
                                                              .ToListAsync();
        notifications = notifications.Where(x => (x.MinVersion ?? vmin) <= v && v < (x.MaxVersion ?? vmax))
                                     .OrderByDescending(x => x.Time)
                                     .ToList();
        var wrapper = new NotificationWrapper<NotificationServerModel>
        {
            Platform = PlatformType.Desktop,
            Channel = channel,
            Version = v,
            List = notifications,
        };
        return wrapper;
    }




    [NoWrapper]
    [HttpGet("TitleBarText")]
    public string GetTitleBarText()
    {
        return "";
    }


    [HttpGet("InfoBar")]
    [ResponseCache(Duration = 900)]
    public async Task<object> GetHomePageInfoBarsAsync()
    {
        using var dapper = _factory.CreateDbConnection();
        var list = await dapper.QueryAsync<InfoBarContent>("SELECT * FROM InfoBarContent;");
        return new { List = list };
    }




}



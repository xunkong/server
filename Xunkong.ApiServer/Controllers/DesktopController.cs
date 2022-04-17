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


    public DesktopController(ILogger<DesktopController> logger, XunkongDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
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


    [NoWrapper]
    [HttpGet("AppInstaller")]
    public async Task<ActionResult<string>> GetAppInstallerContentAsync([FromQuery] ChannelType channel = ChannelType.All, [FromQuery] string? version = null)
    {
        Version.TryParse(version, out var v);
        var vm = await _dbContext.DesktopUpdateVersions.AsNoTracking().Where(x => x.Version == v).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
        if (vm is null)
        {
            vm = await _dbContext.DesktopUpdateVersions.AsNoTracking().Where(x => x.Channel.HasFlag(channel)).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
        }
        if (vm is null)
        {
            return NotFound();
        }
        else
        {
            return AppInstallerTemplate.Replace("{AppVersion}", vm.Version.ToString());
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



    private const string AppInstallerTemplate = """
        <?xml version="1.0" encoding="utf-8"?>
        <AppInstaller
        	Uri="https://api.xunkong.cc/v0.1/desktop/appinstaller?version={AppVersion}"
        	Version="{AppVersion}" xmlns="http://schemas.microsoft.com/appx/appinstaller/2017/2">
        	<MainBundle
        		Name="Xunkong.Desktop"
        		Version="{AppVersion}"
        		Publisher="CN=Xunkong by Scighost"
        		Uri="https://file.xunkong.cc/download/package/Xunkong.Desktop.Package_{AppVersion}_x64.msixbundle" />
        	<Dependencies>
        		<Package
        			Name="Microsoft.VCLibs.140.00"
        			Publisher="CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US"
        			ProcessorArchitecture="x64"
        			Uri="https://file.xunkong.cc/download/package/Microsoft.VCLibs.x64.14.00.appx"
        			Version="14.0.30704.0" />
        		<Package
        			Name="Microsoft.VCLibs.140.00.UWPDesktop"
        			Publisher="CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US"
        			ProcessorArchitecture="x64"
        			Uri="https://file.xunkong.cc/download/package/Microsoft.VCLibs.x64.14.00.Desktop.appx"
        			Version="14.0.30704.0" />
        		<Package
        			Name="Microsoft.WindowsAppRuntime.1.0"
        			Publisher="CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US"
        			ProcessorArchitecture="x64"
        			Uri="https://file.xunkong.cc/download/package/Microsoft.WindowsAppRuntime.1.0.msix"
        			Version="2.460.358.0" />
        	</Dependencies>
        </AppInstaller>
        """;


}



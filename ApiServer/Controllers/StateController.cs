namespace Xunkong.ApiServer.Controllers;

[Route("[controller]")]
[ApiController]
[ApiVersion("0")]
[ApiVersionNeutral]
public class StateController : ControllerBase
{


    public static DateTime StartTime;

    private static DateTime lastUpdateTime = new FileInfo(typeof(StateController).Assembly.Location).CreationTime;


    public StateController()
    {
    }


    /// <summary>
    /// 返回函数实例的状态，并保持函数实例长时间运行
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [NoWrapper]
    [ResponseCache(NoStore = true)]
    public object GetState()
    {
        return new { Code = 0, Message = "Api instance has started.", Data = new { LastUpdateTime = lastUpdateTime, RunningTime = DateTime.Now - StartTime } };
    }


    [HttpPost("ClearCache")]
    public object ClearCache()
    {
        return new { Code = 0, Message = "Cache cleared." };
    }


}

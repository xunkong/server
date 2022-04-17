namespace Xunkong.ApiClient;

public class WallpaperInfoListWrapper
{

    public WallpaperInfoListWrapper() { }

    public WallpaperInfoListWrapper(int page, int totalPage, int count, List<WallpaperInfo> list)
    {
        Page = page;
        TotalPage = totalPage;
        Count = count;
        List = list;
    }

    public int Page { get; set; }

    public int TotalPage { get; set; }

    public int Count { get; set; }

    public List<WallpaperInfo> List { get; set; }

}
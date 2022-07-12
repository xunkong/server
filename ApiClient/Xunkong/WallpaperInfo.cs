namespace Xunkong.ApiClient;

public class WallpaperInfo
{

    public int Id { get; set; }

    public bool Enable { get; set; }

    public bool Recommend { get; set; }

    public string? Title { get; set; }

    public string? Author { get; set; }

    public string? Description { get; set; }

    public List<string> Tags { get; set; }

    public string Url { get; set; }

    public string? Source { get; set; }


    public string? FileName { get; set; }

    [JsonIgnore]
    public string SourceDomain => new Uri(Source ?? "").Host;

}

public class File
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string FileUrl { get; set; }
    public string HlsUrl { get; set; }
    public string FileType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}

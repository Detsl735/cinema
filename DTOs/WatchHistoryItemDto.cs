using System;

public class WatchHistoryItemDto
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = default!;
    public DateTime WatchedAt { get; set; }
}

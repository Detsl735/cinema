using System.Collections.Generic;
using System.Threading.Tasks;

public class WatchHistoryService
{
    private readonly WatchHistoryRepository _repo;

    public WatchHistoryService(WatchHistoryRepository repo)
    {
        _repo = repo;
    }

    public Task AddAsync(int movieId, int userId)
        => _repo.AddAsync(userId, movieId);

    public Task<IEnumerable<WatchHistoryItemDto>> GetWithTitlesAsync(int userId)
        => _repo.GetWithTitlesAsync(userId);
}

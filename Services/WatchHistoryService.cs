using System.Collections.Generic;
using System.Threading.Tasks;

public class WatchHistoryService
{
    private readonly WatchHistoryRepository _repository;

    public WatchHistoryService(WatchHistoryRepository repository)
    {
        _repository = repository;
    }

    public async Task AddHistoryAsync(WatchHistory history)
    {
        await _repository.AddHistoryAsync(history);
    }

    public async Task<IEnumerable<WatchHistory>> GetHistoryByUserAsync(int userId)
    {
        return await _repository.GetHistoryByUserAsync(userId);
    }

    public async Task UpdateProgressAsync(int id, int progress)
    {
        await _repository.UpdateProgressAsync(id, progress);
    }
}

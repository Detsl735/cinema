using System.Collections.Generic;
using System.Threading.Tasks;

public class UserService
{
    private readonly UserRepository _repository;

    public UserService(UserRepository repository)
    {
        _repository = repository;
    }

    public async Task AddUserAsync(User user)
    {
        await _repository.AddUserAsync(user);
    }

}

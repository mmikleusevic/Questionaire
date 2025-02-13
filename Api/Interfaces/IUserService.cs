using QuestionaireApi.Models.Database;

namespace QuestionaireApi.Interfaces;

public interface IUserService
{
    Task<List<User>> GetUsersAsync();
    Task<User?> GetUserByIdAsync(int userId);
    Task CreateUserAsync(User user);
    Task<bool> UpdateUserAsync(int userId, User updatedUser);
    Task<bool> DeleteUserAsync(int userId);
}
using QuestionaireApi.Models.Database;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Interfaces;

public interface IUserService
{
    Task<List<User>> GetUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task CreateUserAsync(User user);
    Task<bool> UpdateUserAsync(int id, User updatedUser);
    Task<bool> DeleteUserAsync(int id);
}
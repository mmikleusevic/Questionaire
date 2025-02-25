using QuestionaireApi.Models.Database;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Interfaces;

public interface IUserService
{
    Task<List<User>> GetUsers();
    Task<UserDto?> GetUserById(int id);
    Task CreateUser(User user);
    Task<bool> UpdateUser(int id, User updatedUser);
    Task<bool> DeleteUser(int id);
}
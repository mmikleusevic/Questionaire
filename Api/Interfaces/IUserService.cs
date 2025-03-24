using Shared.Models;

namespace QuestionaireApi.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetUsers();
    Task<bool> UpdateUser(UserDto updatedUser);
    Task<bool> DeleteUser(string username);
}
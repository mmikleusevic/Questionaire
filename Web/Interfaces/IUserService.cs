using Shared.Models;

namespace Web.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetUsers();
    Task UpdateUser(UserDto updatedUser);
    Task DeleteUser(string email);
}
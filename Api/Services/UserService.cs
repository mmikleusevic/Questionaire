using Microsoft.AspNetCore.Identity;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using Shared.Models;

namespace QuestionaireApi.Services;

public class UserService(UserManager<User> userManager) : IUserService
{
    public async Task<List<UserDto>> GetUsers()
    {
        try
        {
            List<User> users = userManager.Users.ToList();

            List<UserDto> userDtos = new List<UserDto>();

            foreach (User user in users)
            {
                IList<string>? roles = await userManager.GetRolesAsync(user);

                List<RoleDto> rolesDto = new List<RoleDto>();

                foreach (string role in roles)
                {
                    rolesDto.Add(new RoleDto
                    {
                        RoleName = role
                    });
                }

                userDtos.Add(new UserDto
                {
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Roles = rolesDto
                });
            }

            return userDtos;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving users.", ex);
        }
    }

    public async Task<bool> UpdateUser(UserDto updatedUser)
    {
        try
        {
            User? user = await userManager.FindByNameAsync(updatedUser.UserName);
            if (user == null) return false;

            IList<string> currentRoles = await userManager.GetRolesAsync(user);

            await userManager.RemoveFromRolesAsync(user, currentRoles);

            IList<string> newRoles = updatedUser.Roles?.Select(a => a.RoleName).ToList() ?? new List<string>();

            await userManager.AddToRolesAsync(user, newRoles);

            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"An error occurred while updating user with username {updatedUser.UserName}.", ex);
        }
    }

    public async Task<bool> DeleteUser(string username)
    {
        try
        {
            User? user = await userManager.FindByNameAsync(username);
            if (user == null) return false;

            IdentityResult result = await userManager.DeleteAsync(user);

            return result.Succeeded;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while deleting user with username {username}.", ex);
        }
    }
}
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

                userDtos.Add(new UserDto
                {
                    Email = user.Email ?? string.Empty,
                    Roles = roles
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
        User? user = await userManager.FindByEmailAsync(updatedUser.Email);
        if (user == null) return false;

        IList<string> currentRoles = await userManager.GetRolesAsync(user);

        await userManager.RemoveFromRolesAsync(user, currentRoles);
        await userManager.AddToRolesAsync(user, updatedUser.Roles);

        return true;
    }

    public async Task<bool> DeleteUser(string email)
    {
        User? user = await userManager.FindByEmailAsync(email);
        if (user == null) return false;

        IdentityResult result = await userManager.DeleteAsync(user);

        return result.Succeeded;
    }
}
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using Shared.Models;

namespace QuestionaireApi.Services;

public class UserService(
    UserManager<User> userManager,
    QuestionaireDbContext context) : IUserService
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
        IExecutionStrategy strategy = context.Database.CreateExecutionStrategy();

        try
        {
            return await strategy.ExecuteAsync(async () =>
            {
                await using IDbContextTransaction? transaction = await context.Database.BeginTransactionAsync();

                string defaultUserName = "admin";
                User? defaultUser = await userManager.FindByNameAsync(defaultUserName);

                if (defaultUser == null) return false;

                string defaultUserId = defaultUser.Id;

                var userToDelete = await userManager.FindByNameAsync(username);
                if (userToDelete == null) return false;

                string userIdToDelete = userToDelete.Id;

                if (userIdToDelete == defaultUserId) return false;

                List<Question> questionsToReassign = await context.Questions
                    .Where(q => q.CreatedById == userIdToDelete ||
                                q.LastUpdatedById == userIdToDelete ||
                                q.ApprovedById == userIdToDelete ||
                                q.DeletedById == userIdToDelete)
                    .ToListAsync();

                bool requiresSaveChanges = false;
                foreach (var question in questionsToReassign)
                {
                    if (question.CreatedById == userIdToDelete)
                    {
                        question.CreatedById = defaultUserId;
                        requiresSaveChanges = true;
                    }

                    if (question.LastUpdatedById == userIdToDelete)
                    {
                        question.LastUpdatedById = defaultUserId;
                        requiresSaveChanges = true;
                    }

                    if (question.ApprovedById == userIdToDelete)
                    {
                        question.ApprovedById = defaultUserId;
                        requiresSaveChanges = true;
                    }

                    if (question.DeletedById == userIdToDelete)
                    {
                        question.DeletedById = defaultUserId;
                        requiresSaveChanges = true;
                    }
                }

                if (requiresSaveChanges)
                {
                    await context.SaveChangesAsync();
                }

                IdentityResult deleteResult = await userManager.DeleteAsync(userToDelete);

                if (deleteResult.Succeeded)
                {
                    await transaction.CommitAsync();
                }
                else
                {
                    await transaction.RollbackAsync();
                }

                return deleteResult.Succeeded;
            });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"An unexpected error occurred while deleting user '{username}' and reassigning content.", ex);
        }
    }
}
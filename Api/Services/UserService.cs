using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using System;

namespace QuestionaireApi.Services;

public class UserService(QuestionaireDbContext context) : IUserService
{
    public async Task<List<User>> GetUsersAsync()
    {
        try
        {
            return await context.Users
                .Include(u => u.Role)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while fetching users.", ex);
        }
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        try
        {
            return await context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"An error occurred while fetching the user with ID {userId}.", ex);
        }
    }

    public async Task CreateUserAsync(User user)
    {
        try
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while creating the user.", ex);
        }
    }

    public async Task<bool> UpdateUserAsync(int userId, User updatedUser)
    {
        try
        {
            User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return false;

            user.Name = updatedUser.Name;
            user.Password = updatedUser.Password;
            user.RoleId = updatedUser.RoleId;

            await context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"An error occurred while updating the user with ID {userId}.", ex);
        }
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        try
        {
            User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return false;

            context.Users.Remove(user);
            await context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"An error occurred while deleting the user with ID {userId}.", ex);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using System;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Services;

public class UserService(QuestionaireDbContext context) : IUserService
{
    public async Task<List<User>> GetUsers()
    {
        try
        {
            return await context.Users
                .Include(u => u.Role)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while fetching users.", ex);
        }
    }

    public async Task<UserDto?> GetUserById(int id)
    {
        try
        {
            return await context.Users
                .Include(u => u.Role)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    RoleId = u.RoleId,
                })
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while fetching the user with ID {id}.", ex);
        }
    }

    public async Task CreateUser(User user)
    {
        try
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while creating the user.", ex);
        }
    }

    public async Task<bool> UpdateUser(int id, User updatedUser)
    {
        try
        {
            User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return false;

            user.Name = updatedUser.Name;
            user.Password = updatedUser.Password;
            user.RoleId = updatedUser.RoleId;

            await context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while updating the user with ID {id}.", ex);
        }
    }

    public async Task<bool> DeleteUser(int id)
    {
        try
        {
            User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return false;

            context.Users.Remove(user);
            await context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while deleting the user with ID {id}.", ex);
        }
    }
}

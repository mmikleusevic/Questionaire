using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;

namespace QuestionaireApi.Services;

public class UserService(UserManager<User> userManager) : IUserService
{
    public async Task<string> GetUserId(ClaimsPrincipal user)
    {
        string? userId = await Task.FromResult(userManager.GetUserId(user));
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        
        return userId;
    }
}
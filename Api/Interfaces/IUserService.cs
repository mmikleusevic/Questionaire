using System.Security.Claims;

namespace QuestionaireApi.Interfaces;

public interface IUserService
{
    Task<string> GetUserId(ClaimsPrincipal user);
}
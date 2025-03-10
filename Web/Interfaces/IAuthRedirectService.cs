using System.Security.Claims;

namespace Web.Interfaces;

public interface IAuthRedirectService
{
    Task CheckAndRedirect(ClaimsPrincipal? user);
}
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Tests.Helper;

public static class UserManagerMockHelper
{
    public static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        var options = new Mock<IOptions<IdentityOptions>>();
        var pwdHasher = new Mock<IPasswordHasher<TUser>>();
        var userValidators = new List<IUserValidator<TUser>>();
        var pwdValidators = new List<IPasswordValidator<TUser>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<TUser>>>();

        var mock = new Mock<UserManager<TUser>>(
            store.Object, options.Object, pwdHasher.Object, userValidators, pwdValidators,
            keyNormalizer.Object, errors.Object, services.Object, logger.Object);

        mock.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((ClaimsPrincipal cp) =>
            cp.FindFirstValue(ClaimTypes.NameIdentifier));

        mock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((ClaimsPrincipal cp) => null);

        return mock;
    }
}
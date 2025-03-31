using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;

namespace Tests.Helper;

public static class RoleManagerMockHelper
{
    public static Mock<RoleManager<TRole>> MockRoleManager<TRole>(List<TRole> roles) where TRole : class
    {
        var store = new Mock<IRoleStore<TRole>>();
        var roleValidators = new List<IRoleValidator<TRole>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var logger = new Mock<ILogger<RoleManager<TRole>>>();

        var mock = new Mock<RoleManager<TRole>>(
            store.Object, roleValidators, keyNormalizer.Object, errors.Object, logger.Object);

        IQueryable<TRole> queryableRoles = roles.AsQueryable().BuildMock();

        mock.Setup(m => m.Roles).Returns(queryableRoles);

        return mock;
    }
}
using Microsoft.AspNetCore.Identity.Data;

namespace QuestionaireApi.IdentityApi;

public class ExtendedInfoResponse
{
    public required string Email { get; init; }
    public required string Name { get; init; }
    public required bool IsEmailConfirmed { get; init; }
    public required IList<string> Roles { get; init; }
}
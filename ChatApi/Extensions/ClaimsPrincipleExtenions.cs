using System.Security.Claims;

namespace ChatApi.Extensions;

public static class ClaimsPrincipleExtensions
{
    public static string GetUserName(this ClaimsPrincipal user)
    {
        var username = user.FindFirstValue(ClaimTypes.Name) ?? throw new Exception("Cannot get username");

        return username;
    }

    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ??
                                throw new Exception("Cannot get userId"));
        return userId;
    }
}
using System.Security.Claims;

namespace Api.Auth;

public static class AuthConstants
{
    public const string CookieScheme = "IndkobCookie";
    public const string AdminPolicy = "AdminOnly";

    public const string ClaimUserId = "indkob:userId";
    public const string ClaimIsAdmin = "indkob:isAdmin";

    public static ClaimsIdentity CreateIdentity(int userId, string userName, bool isAdmin)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userName),
            new(ClaimUserId, userId.ToString()),
            new(ClaimIsAdmin, isAdmin ? "true" : "false")
        };

        return new ClaimsIdentity(claims, CookieScheme);
    }
}


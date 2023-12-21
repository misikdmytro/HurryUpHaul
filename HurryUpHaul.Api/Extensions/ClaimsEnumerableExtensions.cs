using System.Security.Claims;

namespace HurryUpHaul.Api.Extensions
{
    internal static class ClaimsEnumerableExtensions
    {
        public static IEnumerable<string> Roles(this IEnumerable<Claim> claims)
        {
            return claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
        }
    }
}
using System.Security.Claims;

using HurryUpHaul.Contracts.Models;
using HurryUpHaul.Domain.Constants;

namespace HurryUpHaul.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool IsInRole(this ClaimsPrincipal user, string role)
        {
            return user.Claims.Roles().Any(r => r == role);
        }

        public static bool CanSeeOrder(this ClaimsPrincipal user, Order order, string[] managersList)
        {
            return order.CreatedBy == user.Identity.Name ||
                IsInRole(user, Roles.Admin) ||
                managersList.Contains(user.Identity.Name);
        }

        public static bool CanSeeRestaurantDetails(this ClaimsPrincipal user, Restaurant restaurant)
        {
            return user.IsInRole(Roles.Admin) ||
                restaurant.Managers.Contains(user.Identity.Name);
        }
    }
}
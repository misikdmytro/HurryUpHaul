using HurryUpHaul.Contracts.Models;

namespace HurryUpHaul.Contracts.Http
{
    public class GetRestaurantOrdersResponse
    {
        public IEnumerable<Order> Orders { get; init; }
    }
}
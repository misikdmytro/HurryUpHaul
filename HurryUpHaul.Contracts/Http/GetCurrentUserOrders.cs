using HurryUpHaul.Contracts.Models;

namespace HurryUpHaul.Contracts.Http
{
    public class GetCurrentUserOrdersResponse
    {
        public IEnumerable<Order> Orders { get; init; }
    }
}
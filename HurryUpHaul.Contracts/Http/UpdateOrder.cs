using HurryUpHaul.Contracts.Models;

namespace HurryUpHaul.Contracts.Http
{
    public class UpdateOrderRequest
    {
        public OrderStatus Status { get; init; }
    }
}
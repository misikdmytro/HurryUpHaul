using HurryUpHaul.Contracts.Models;

namespace HurryUpHaul.Contracts.Http
{
    public class UpdateOrderRequest
    {
        public string OrderId { get; init; }
        public OrderStatus Status { get; init; }
    }
}
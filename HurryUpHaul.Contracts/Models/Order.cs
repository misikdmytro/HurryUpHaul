namespace HurryUpHaul.Contracts.Models
{
    public class Order
    {
        public string Id { get; init; }
        public string Details { get; init; }
        public OrderStatus Status { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public string CreatedBy { get; init; }
        public DateTimeOffset LastUpdatedAt { get; init; }
    }
}
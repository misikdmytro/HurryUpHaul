namespace HurryUpHaul.Contracts.Models
{
    public class Order
    {
        public Guid Id { get; init; }
        public string Details { get; init; }
        public OrderStatus Status { get; init; }
        public DateTime CreatedAt { get; init; }
        public string CreatedBy { get; init; }
        public DateTime LastUpdatedAt { get; init; }
    }
}
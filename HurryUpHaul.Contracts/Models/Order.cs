namespace HurryUpHaul.Contracts.Models
{
    public class Order
    {
        public string Id { get; init; }
        public string Details { get; init; }
        public string Status { get; init; }
        public DateTime CreatedAt { get; init; }
        public string CreatedBy { get; init; }
        public DateTime LastUpdatedAt { get; init; }
    }
}
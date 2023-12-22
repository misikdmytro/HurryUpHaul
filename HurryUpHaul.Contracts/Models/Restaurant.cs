namespace HurryUpHaul.Contracts.Models
{
    public class Restaurant
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public DateTimeOffset? CreatedAt { get; init; }
        public IEnumerable<string> Managers { get; init; }
    }
}
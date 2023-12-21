namespace HurryUpHaul.Contracts.Models
{
    public class Restaurant
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public DateTime? CreatedAt { get; init; }
        public string[] Managers { get; init; }
    }
}
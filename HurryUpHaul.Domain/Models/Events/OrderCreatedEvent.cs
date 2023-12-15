namespace HurryUpHaul.Domain.Models.Events
{
    internal record OrderCreatedEvent
    {
        public string Details { get; init; }
    }
}
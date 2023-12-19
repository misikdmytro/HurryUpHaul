namespace HurryUpHaul.Contracts.Http
{
    public class CreateOrderRequest
    {
        public Guid RestaurantId { get; init; }
        public string Details { get; init; }
    }

    public class CreateOrderResponse
    {
        public Guid Id { get; init; }
    }
}
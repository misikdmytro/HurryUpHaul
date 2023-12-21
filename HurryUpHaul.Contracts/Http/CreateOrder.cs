namespace HurryUpHaul.Contracts.Http
{
    public class CreateOrderRequest
    {
        public string RestaurantId { get; init; }
        public string Details { get; init; }
    }

    public class CreateOrderResponse
    {
        public string OrderId { get; init; }
    }
}
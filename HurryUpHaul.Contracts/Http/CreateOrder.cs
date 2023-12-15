namespace HurryUpHaul.Contracts.Http
{
    public class CreateOrderRequest
    {
        public string Details { get; init; }
    }

    public class CreateOrderResponse
    {
        public string Id { get; init; }
    }
}
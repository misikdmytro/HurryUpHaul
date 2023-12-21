namespace HurryUpHaul.Contracts.Http
{
    public class CreateRestaurantRequest
    {
        public string Name { get; init; }
        public string[] ManagersIds { get; init; }
    }

    public class CreateRestaurantResponse
    {
        public string RestaurantId { get; init; }
    }
}
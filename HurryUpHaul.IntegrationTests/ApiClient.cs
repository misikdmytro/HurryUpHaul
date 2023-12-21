using Flurl;
using Flurl.Http;

using HurryUpHaul.Contracts.Http;

namespace HurryUpHaul.IntegrationTests
{
    internal interface IApiClient : IDisposable
    {
        Task<CreateRestaurantResponse> CreateRestaurant(CreateRestaurantRequest request, string token);
        Task<GetRestaurantResponse> GetRestaurant(string restaurantId);
        Task<GetRestaurantResponse> GetRestaurant(string restaurantId, string token);
        Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request);
        Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, string token);
        Task<GetOrderResponse> GetOrder(string orderId, string token);
        Task<RegisterUserResponse> RegisterUser(RegisterUserRequest request);
        Task<AuthenticateUserResponse> AuthenticateUser(AuthenticateUserRequest request);
        Task<MeResponse> Me(string token);
        Task AdminUpdate(AdminUpdateUserRequest request, string token);
    }

    internal class ApiClient : IApiClient
    {
        private readonly FlurlClient _client;

        public ApiClient(HttpClient client)
        {
            _client = new FlurlClient(client);
        }

        public Task<AuthenticateUserResponse> AuthenticateUser(AuthenticateUserRequest request)
        {
            return _client
                .Request()
                .AppendPathSegment("api")
                .AppendPathSegment("users")
                .AppendPathSegment("token")
                .PostJsonAsync(request)
                .ReceiveJson<AuthenticateUserResponse>();
        }

        public Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request)
        {
            return _client
                .Request()
                .AppendPathSegment("api")
                .AppendPathSegment("orders")
                .PostJsonAsync(request)
                .ReceiveJson<CreateOrderResponse>();
        }

        public Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, string token)
        {
            return _client
                .Request()
                .AppendPathSegment("api")
                .AppendPathSegment("orders")
                .WithOAuthBearerToken(token)
                .PostJsonAsync(request)
                .ReceiveJson<CreateOrderResponse>();
        }

        public Task<CreateRestaurantResponse> CreateRestaurant(CreateRestaurantRequest request, string token)
        {
            return _client
                .Request()
                .AppendPathSegment("api")
                .AppendPathSegment("restaurants")
                .WithOAuthBearerToken(token)
                .PostJsonAsync(request)
                .ReceiveJson<CreateRestaurantResponse>();
        }

        public Task<GetRestaurantResponse> GetRestaurant(string restaurantId)
        {
            return _client
                .Request()
                .AppendPathSegment("api")
                .AppendPathSegment("restaurants")
                .AppendPathSegment(restaurantId)
                .GetJsonAsync<GetRestaurantResponse>();
        }

        public Task<GetRestaurantResponse> GetRestaurant(string restaurantId, string token)
        {
            return _client
                .Request()
                .AppendPathSegment("api")
                .AppendPathSegment("restaurants")
                .AppendPathSegment(restaurantId)
                .WithOAuthBearerToken(token)
                .GetJsonAsync<GetRestaurantResponse>();
        }

        public Task<GetOrderResponse> GetOrder(string orderId, string token)
        {
            return _client
                .Request()
                .AppendPathSegment("api")
                .AppendPathSegment("orders")
                .AppendPathSegment(orderId)
                .WithOAuthBearerToken(token)
                .GetJsonAsync<GetOrderResponse>();
        }

        public Task<MeResponse> Me(string token)
        {
            return _client
                .Request()
                .AppendPathSegment("api")
                .AppendPathSegment("users")
                .AppendPathSegment("me")
                .WithOAuthBearerToken(token)
                .GetJsonAsync<MeResponse>();
        }

        public Task<RegisterUserResponse> RegisterUser(RegisterUserRequest request)
        {
            return _client
                .Request()
                .AppendPathSegment("api")
                .AppendPathSegment("users")
                .PostJsonAsync(request)
                .ReceiveJson<RegisterUserResponse>();
        }

        public Task AdminUpdate(AdminUpdateUserRequest request, string token)
        {
            return _client
                .Request()
                .AppendPathSegment("api")
                .AppendPathSegment("users")
                .AppendPathSegment("admin")
                .WithOAuthBearerToken(token)
                .PutJsonAsync(request);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
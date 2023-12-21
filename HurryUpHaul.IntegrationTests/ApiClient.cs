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
                .AllowHttpStatus(200)
                .PostJsonAsync(request)
                .ReceiveJson<AuthenticateUserResponse>();
        }

        public Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request)
        {
            return _client
                .Request()
                .AppendPathSegment("api")
                .AppendPathSegment("orders")
                .AllowHttpStatus(201)
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
                .AllowHttpStatus(201)
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
                .AllowHttpStatus(201)
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
                .AllowHttpStatus(200)
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
                .AllowHttpStatus(200)
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
                .AllowHttpStatus(200)
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
                .AllowHttpStatus(200)
                .GetJsonAsync<MeResponse>();
        }

        public Task<RegisterUserResponse> RegisterUser(RegisterUserRequest request)
        {
            return _client
                .Request()
                .AppendPathSegment("api")
                .AppendPathSegment("users")
                .AllowHttpStatus(200)
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
                .AllowHttpStatus(200)
                .PutJsonAsync(request);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
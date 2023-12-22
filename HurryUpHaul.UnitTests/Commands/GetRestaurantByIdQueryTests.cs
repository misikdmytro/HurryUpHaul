using FluentAssertions;

using HurryUpHaul.Domain.Constants;
using HurryUpHaul.Domain.Models.Database;
using HurryUpHaul.Domain.Queries;

using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using Moq;

namespace HurryUpHaul.UnitTests.Commands
{
    public class GetRestaurantByIdQueryTests : Base
    {
        private readonly IRequestHandler<GetRestaurantByIdQuery, GetRestaurantByIdQueryResult> _handler;

        public GetRestaurantByIdQueryTests() : base()
        {
            _handler = new GetRestaurantByIdQueryHandler(_appDbContext, _mapper, Mock.Of<ILogger<GetRestaurantByIdQueryHandler>>());
        }

        [Fact]
        public async Task HandleShouldReturnEmptyResultIfRestaurantDoesNotExist()
        {
            // Arrange
            var query = new GetRestaurantByIdQuery
            {
                RestaurantId = Guid.NewGuid().ToString(),
                Requester = _faker.Person.UserName,
                RequesterRoles = [Roles.Admin]
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Restaurant.Should().BeNull();
        }

        [Fact]
        public async Task HandleShouldReturnRestaurantForAdmin()
        {
            // Arrange
            var restaurant = new Restaurant
            {
                Id = Guid.NewGuid().ToString(),
                Name = _faker.Company.CompanyName(),
                CreatedAt = _faker.Date.RecentOffset(),
                Managers = new List<IdentityUser>
                {
                    new() {
                        UserName = _faker.Person.UserName
                    }
                }
            };

            _appDbContext.Restaurants.Add(restaurant);
            await _appDbContext.SaveChangesAsync();

            var query = new GetRestaurantByIdQuery
            {
                RestaurantId = restaurant.Id,
                Requester = _faker.Person.UserName,
                RequesterRoles = [Roles.Admin]
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Restaurant.Should().NotBeNull();
            result.Restaurant.Id.Should().Be(restaurant.Id);
            result.Restaurant.Name.Should().Be(restaurant.Name);
            result.Restaurant.Managers.Should().BeEquivalentTo(restaurant.Managers.Select(m => m.UserName));
            result.Restaurant.CreatedAt.Should().Be(restaurant.CreatedAt);
        }

        [Fact]
        public async Task HandleShouldReturnRestaurantForManager()
        {
            // Arrange
            var restaurant = new Restaurant
            {
                Id = Guid.NewGuid().ToString(),
                Name = _faker.Company.CompanyName(),
                CreatedAt = _faker.Date.RecentOffset(),
                Managers = new List<IdentityUser>
                {
                    new() {
                        UserName = _faker.Person.UserName
                    }
                }
            };

            _appDbContext.Restaurants.Add(restaurant);
            await _appDbContext.SaveChangesAsync();

            var query = new GetRestaurantByIdQuery
            {
                RestaurantId = restaurant.Id,
                Requester = restaurant.Managers.First().UserName,
                RequesterRoles = [Roles.User]
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Restaurant.Should().NotBeNull();
            result.Restaurant.Id.Should().Be(restaurant.Id);
            result.Restaurant.Name.Should().Be(restaurant.Name);
            result.Restaurant.Managers.Should().BeEquivalentTo(restaurant.Managers.Select(m => m.UserName));
            result.Restaurant.CreatedAt.Should().Be(restaurant.CreatedAt);
        }

        [Fact]
        public async Task HandleShouldReturnRestaurantForUser()
        {
            // Arrange
            var restaurant = new Restaurant
            {
                Id = Guid.NewGuid().ToString(),
                Name = _faker.Company.CompanyName(),
                CreatedAt = _faker.Date.RecentOffset()
            };

            _appDbContext.Restaurants.Add(restaurant);
            await _appDbContext.SaveChangesAsync();

            var query = new GetRestaurantByIdQuery
            {
                RestaurantId = restaurant.Id,
                Requester = _faker.Person.UserName,
                RequesterRoles = [Roles.User]
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Restaurant.Should().NotBeNull();
            result.Restaurant.Id.Should().Be(restaurant.Id);
            result.Restaurant.Name.Should().Be(restaurant.Name);
            result.Restaurant.Managers.Should().BeNull();
            result.Restaurant.CreatedAt.Should().BeNull();
        }
    }
}
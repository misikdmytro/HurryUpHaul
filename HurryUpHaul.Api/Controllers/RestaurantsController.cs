using System.Net.Mime;
using System.Security.Claims;

using FluentValidation;

using HurryUpHaul.Contracts.Http;
using HurryUpHaul.Domain.Commands;
using HurryUpHaul.Domain.Queries;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HurryUpHaul.Api.Controllers
{
    [Route("api/restaurants")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    public class RestaurantsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IValidator<CreateRestaurantRequest> _createRestaurantValidator;

        public RestaurantsController(IMediator mediator, IValidator<CreateRestaurantRequest> createRestaurantValidator)
        {
            _mediator = mediator;
            _createRestaurantValidator = createRestaurantValidator;
        }

        /// <summary>
        /// Creates a new restaurant
        /// </summary>
        /// <param name="request">Create restaurant request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created restaurant ID</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///    POST /api/restaurants
        ///    {
        ///     name: "Test Restaurant Name"
        ///     managersIds: ["manager1", "manager2"]
        ///    }
        /// 
        /// </remarks>
        /// <response code="201">Restaurant created</response>
        /// <response code="400">Invalid request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [Authorize(Policy = "Admin")]
        [ProducesResponseType(typeof(CreateRestaurantResponse), 201)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 403)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> CreateRestaurant([FromBody] CreateRestaurantRequest request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _createRestaurantValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray()
                });
            }

            var command = new CreateRestaurantCommand
            {
                Name = request.Name,
                ManagersIds = request.ManagersIds
            };

            var result = await _mediator.Send(command, cancellationToken);

            return result.Errors?.Length > 0
                ? BadRequest(result.Errors)
                : Created($"/api/restaurants/{result.RestaurantId}", new CreateRestaurantResponse
                {
                    RestaurantId = result.RestaurantId
                });
        }

        /// <summary>
        /// Returns a restaurant by ID
        /// </summary>
        /// <param name="id">Restaurant ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Restaurant</returns>
        /// <remarks>
        /// Sample request:
        /// 
        /// GET /api/restaurants/{id}
        /// 
        /// </remarks>
        /// <response code="200">Restaurant found</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Restaurant not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GetRestaurantResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 403)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> GetRestaurantById([FromRoute] string id, CancellationToken cancellationToken = default)
        {
            var query = new GetRestaurantByIdQuery
            {
                RequesterUsername = User?.Identity?.Name,
                RequesterRoles = User?.Claims?.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray(),
                RestaurantId = id
            };

            var result = await _mediator.Send(query, cancellationToken);

            return result.Restaurant == null
                ? NotFound(new ErrorResponse
                {
                    Errors = new[] { $"Restaurant with ID '{id}' not found." }
                })
                : Ok(new GetRestaurantResponse
                {
                    Restaurant = result.Restaurant
                });
        }
    }
}
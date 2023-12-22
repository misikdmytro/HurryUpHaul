using System.Net;
using System.Net.Mime;

using FluentValidation;

using HurryUpHaul.Api.Extensions;
using HurryUpHaul.Contracts.Http;
using HurryUpHaul.Contracts.Models;
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

            return result.Result switch
            {
                CreateRestaurantCommandResultType.ManagersNotFound => BadRequest(new ErrorResponse
                {
                    Errors = result.Errors
                }),
                CreateRestaurantCommandResultType.Success => Created($"/api/restaurants/{result.RestaurantId}", new CreateRestaurantResponse
                {
                    RestaurantId = result.RestaurantId
                }),
                _ => throw new ArgumentOutOfRangeException(nameof(request), result.Result, "Unexpected result type.")
            };
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
                RestaurantId = id
            };

            var result = await _mediator.Send(query, cancellationToken);

            return result.Result == GetRestaurantByIdQueryResultType.RestaurantNotFound
                ? NotFound(new ErrorResponse
                {
                    Errors = result.Errors
                })
                : result.Result == GetRestaurantByIdQueryResultType.Success
                ? User.CanSeeRestaurantDetails(result.Restaurant)
                    ? Ok(new GetRestaurantResponse
                    {
                        Restaurant = result.Restaurant
                    })
                    : Ok(new GetRestaurantResponse
                    {
                        Restaurant = new Restaurant
                        {
                            Id = result.Restaurant.Id,
                            Name = result.Restaurant.Name
                        }
                    })
                : throw new ArgumentOutOfRangeException(nameof(id), result.Result, "Unexpected result type.");
        }

        /// <summary>
        /// Returns a list of orders for a restaurant
        /// </summary>
        /// <param name="id">Restaurant ID</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="pageNumber">Page number</param> 
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of orders</returns>
        /// <remarks>
        /// Sample request:
        /// 
        /// GET /api/restaurants/{id}/orders?pageSize=10&amp;pageNumber=1
        /// 
        /// </remarks>
        /// <response code="200">Orders found</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Restaurant not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}/orders")]
        [ProducesResponseType(typeof(GetRestaurantOrdersResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 403)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> GetRestaurantOrders([FromRoute] string id,
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageNumber = 1,
            CancellationToken cancellationToken = default)
        {
            if (pageSize < 1 || pageNumber < 1 || pageSize > 1000)
            {
                List<string> errors = [];
                if (pageSize < 1)
                {
                    errors.Add("Page size must be greater than 0.");
                }

                if (pageNumber < 1)
                {
                    errors.Add("Page number must be greater than 0.");
                }

                if (pageSize > 1000)
                {
                    errors.Add("Page size must be less than or equal to 1000.");
                }

                return BadRequest(new ErrorResponse
                {
                    Errors = errors
                });
            }

            var query = new GetRestaurantOrdersQuery
            {
                RestaurantId = id,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query, cancellationToken);

            return result.Result == GetRestaurantOrdersQueryResultType.RestaurantNotFound
                ? NotFound(new ErrorResponse
                {
                    Errors = result.Errors
                })
                : result.Result == GetRestaurantOrdersQueryResultType.Success
                ? User.CanSeeRestaurantDetails(result.Restaurant)
                    ? Ok(new GetRestaurantOrdersResponse
                    {
                        Orders = result.Orders
                    })
                    : (IActionResult)StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse
                    {
                        Errors = ["You are not authorized to view this restaurant's orders."]
                    })
                : throw new ArgumentOutOfRangeException(nameof(id), result.Result, "Unexpected result type.");
        }
    }
}
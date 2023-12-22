using System.Net.Mime;

using FluentValidation;

using HurryUpHaul.Api.Constants;
using HurryUpHaul.Api.Extensions;
using HurryUpHaul.Contracts.Http;
using HurryUpHaul.Domain.Commands;
using HurryUpHaul.Domain.Queries;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HurryUpHaul.Api.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IValidator<CreateOrderRequest> _createOrderValidator;

        public OrdersController(IMediator mediator, IValidator<CreateOrderRequest> createOrderValidator)
        {
            _mediator = mediator;
            _createOrderValidator = createOrderValidator;
        }

        /// <summary>
        /// Creates a new order
        /// </summary>
        /// <param name="request">Create order request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created order ID</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///    POST /api/orders
        ///   {
        ///     details: "Test Order Details"
        ///   }
        /// 
        /// </remarks>
        /// <response code="201">Order created</response>
        /// <response code="400">Invalid request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [Authorize(Policy = AuthorizePolicies.User)]
        [ProducesResponseType(typeof(CreateOrderResponse), 201)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 403)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _createOrderValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Errors = validationResult.Errors.Select(x => x.ErrorMessage).ToArray()
                });
            }

            var command = new CreateOrderCommand
            {
                RestaurantId = request.RestaurantId,
                Customer = User.Identity.Name,
                OrderDetails = request.Details
            };

            var result = await _mediator.Send(command, cancellationToken);

            return result.Errors?.Length > 0
                ? BadRequest(new ErrorResponse
                {
                    Errors = result.Errors
                })
                : Created($"/api/orders/{result.OrderId}", new CreateOrderResponse
                {
                    OrderId = result.OrderId
                });
        }

        /// <summary>
        /// Gets an order by ID
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Order</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///   GET /api/orders/12345678-1234-1234-1234-123456789012
        /// 
        /// </remarks>
        /// <response code="200">Order found</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Order not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GetOrderResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 403)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> GetOrderById([FromRoute] string id, CancellationToken cancellationToken = default)
        {
            var query = new GetOrderByIdQuery
            {
                OrderId = id,
                Requester = User.Identity.Name,
                RequesterRoles = User.Claims.Roles().ToArray()
            };

            var result = await _mediator.Send(query, cancellationToken);

            return result.Order == null
                ? NotFound(new ErrorResponse
                {
                    Errors = new[] { $"Order with ID '{id}' not found." }
                })
                : Ok(new GetOrderResponse
                {
                    Order = result.Order
                });
        }
    }
}
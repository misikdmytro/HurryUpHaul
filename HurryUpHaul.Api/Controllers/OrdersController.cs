using FluentValidation;

using HurryUpHaul.Contracts.Http;
using HurryUpHaul.Domain.Commands;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace HurryUpHaul.Api.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IValidator<CreateOrderRequest> _validator;

        public OrdersController(IMediator mediator, IValidator<CreateOrderRequest> validator)
        {
            _mediator = mediator;
            _validator = validator;
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
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(CreateOrderCommandResult), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Errors = validationResult.Errors.Select(x => x.ErrorMessage).ToArray()
                });
            }

            var command = new CreateOrderCommand
            {
                OrderDetails = request.Details
            };

            var result = await _mediator.Send(command, cancellationToken);

            return Created($"/api/orders/{result.OrderId}", new CreateOrderResponse
            {
                Id = result.OrderId
            });
        }
    }
}
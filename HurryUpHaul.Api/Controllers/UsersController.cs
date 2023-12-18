using FluentValidation;

using HurryUpHaul.Contracts.Http;
using HurryUpHaul.Domain.Commands;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace HurryUpHaul.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IValidator<RegisterUserRequest> _validator;

        public UsersController(IMediator mediator, IValidator<RegisterUserRequest> validator)
        {
            _mediator = mediator;
            _validator = validator;
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="request">Register user request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Succeeded</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///   POST /api/users
        ///  {
        ///     username: "TestUser",
        ///     password: "TestPassword"
        ///  }
        /// 
        /// </remarks>
        /// <response code="200">User registered</response>
        /// <response code="400">Invalid request</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(RegisterUserResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Errors = validationResult.Errors.Select(x => x.ErrorMessage).ToArray()
                });
            }

            var command = new RegisterUserCommand
            {
                Username = request.Username,
                Password = request.Password
            };

            var result = await _mediator.Send(command, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new ErrorResponse
                {
                    Errors = result.Errors.Select(x => x.Description).ToArray()
                });
            }

            return Ok(new RegisterUserResponse());
        }
    }
}
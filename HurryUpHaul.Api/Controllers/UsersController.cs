using System.Net.Mime;
using System.Security.Claims;

using FluentValidation;

using HurryUpHaul.Contracts.Http;
using HurryUpHaul.Domain.Commands;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HurryUpHaul.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IValidator<RegisterUserRequest> _registerUserValidator;
        private readonly IValidator<AuthenticateUserRequest> _authenticateUserValidator;

        public UsersController(IMediator mediator,
            IValidator<RegisterUserRequest> registerUserValidator,
            IValidator<AuthenticateUserRequest> authenticateUserValidator)
        {
            _mediator = mediator;
            _registerUserValidator = registerUserValidator;
            _authenticateUserValidator = authenticateUserValidator;
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
            var validationResult = await _registerUserValidator.ValidateAsync(request, cancellationToken);
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

        /// <summary>
        /// Authenticates a user
        /// </summary>
        /// <param name="request">Authenticate user request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>JWT Token</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///  POST /api/users/token
        /// {
        ///     username: "TestUser",
        ///     password: "TestPassword"
        /// }
        /// 
        /// </remarks>
        /// <response code="200">User authenticated</response>
        /// <response code="400">Invalid request</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("token")]
        [ProducesResponseType(typeof(AuthenticateUserResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> AuthenticateUser([FromBody] AuthenticateUserRequest request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _authenticateUserValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Errors = validationResult.Errors.Select(x => x.ErrorMessage)
                });
            }

            var command = new AuthenticateUserCommand
            {
                Username = request.Username,
                Password = request.Password
            };

            var result = await _mediator.Send(command, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new ErrorResponse
                {
                    Errors = result.Errors
                });
            }

            return Ok(new AuthenticateUserResponse
            {
                Token = result.Token
            });
        }

        /// <summary>
        /// Returns the current user
        /// </summary>
        /// <returns>Current user</returns>
        /// <remarks>
        /// Sample request:
        /// 
        /// GET /api/users/me
        /// 
        /// </remarks>
        /// <response code="200">User authenticated</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(MeResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public IActionResult GetCurrentUser()
        {
            return Ok(new MeResponse
            {
                Username = User.Identity.Name,
                Role = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value
            });
        }
    }
}
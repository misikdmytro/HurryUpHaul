using System.Net.Mime;

using FluentValidation;

using HurryUpHaul.Api.Constants;
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
    [Route("api/users")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IValidator<RegisterUserRequest> _registerUserValidator;
        private readonly IValidator<AuthenticateUserRequest> _authenticateUserValidator;
        private readonly IValidator<AdminUpdateUserRequest> _adminUpdateUserValidator;
        private readonly IValidator<Paging> _pagingValidator;

        public UsersController(IMediator mediator,
            IValidator<RegisterUserRequest> registerUserValidator,
            IValidator<AuthenticateUserRequest> authenticateUserValidator,
            IValidator<AdminUpdateUserRequest> adminUpdateUserValidator,
            IValidator<Paging> pagingValidator)
        {
            _mediator = mediator;
            _registerUserValidator = registerUserValidator;
            _authenticateUserValidator = authenticateUserValidator;
            _adminUpdateUserValidator = adminUpdateUserValidator;
            _pagingValidator = pagingValidator;
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="request">Register user request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Registered user ID</returns>
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
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
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

            return result.Result switch
            {
                RegisterUserCommandResultType.RegistrationFailed => BadRequest(new ErrorResponse
                {
                    Errors = result.Errors
                }),
                RegisterUserCommandResultType.Success => Ok(new RegisterUserResponse
                {
                    UserId = result.UserId
                }),
                _ => throw new ArgumentOutOfRangeException(nameof(request), result.Result, "Unexpected result type.")
            };
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
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
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

            return result.Result switch
            {
                AuthenticateUserCommandResultType.InvalidUsernameOrPassword => BadRequest(new ErrorResponse
                {
                    Errors = result.Errors
                }),
                AuthenticateUserCommandResultType.Success => Ok(new AuthenticateUserResponse
                {
                    Token = result.Token
                }),
                _ => throw new ArgumentOutOfRangeException(nameof(request), result.Result, "Unexpected result type.")
            };
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
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public IActionResult GetCurrentUser()
        {
            return Ok(new MeResponse
            {
                Username = User.Identity.Name,
                Roles = User.Claims.Roles().ToArray(),
            });
        }

        /// <summary>
        /// Updates a user
        /// </summary>
        /// <param name="request">Update user request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>OK</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///  PUT /api/users/admin
        ///  {
        ///     username: "TestUser",
        ///     role: "admin"
        ///  }
        /// 
        /// </remarks>
        /// <response code="200">User updated</response>
        /// <response code="400">Invalid request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("admin")]
        [Authorize(Policy = AuthorizePolicies.Admin)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 403)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> AdminUpdateUser([FromBody] AdminUpdateUserRequest request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _adminUpdateUserValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Errors = validationResult.Errors.Select(x => x.ErrorMessage)
                });
            }

            var command = new AdminUpdateUserCommand
            {
                Username = request.Username,
                RoleToAdd = request.Roles.Where(x => x.Action is UpdateRoleAction.Add).Select(x => x.Role).ToArray(),
                RoleToRemove = request.Roles.Where(x => x.Action is UpdateRoleAction.Remove).Select(x => x.Role).ToArray()
            };

            var result = await _mediator.Send(command, cancellationToken);

            return result.Result switch
            {
                AdminUpdateUserCommandResultType.UpdateFailed or AdminUpdateUserCommandResultType.UserNotFound => BadRequest(new ErrorResponse
                {
                    Errors = result.Errors
                }),
                AdminUpdateUserCommandResultType.Success => Ok(),
                _ => throw new ArgumentOutOfRangeException(nameof(request), result.Result, "Unexpected result type.")
            };
        }

        /// <summary>
        /// Gets all orders for the current user
        /// </summary>
        /// <param name="pageSize">Page size</param>
        /// <param name="pageNumber">Page number</param> 
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of orders</returns>
        /// <remarks>
        /// Sample request:
        /// 
        /// GET /api/users/me/orders?pageSize=10&amp;pageNumber=1 
        ///
        /// </remarks>
        /// <response code="200">Orders found</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("me/orders")]
        [Authorize(Policy = AuthorizePolicies.User)]
        [ProducesResponseType(typeof(GetCurrentUserOrdersResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 403)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> GetUserOrders([FromQuery] int pageSize = 10,
            [FromQuery] int pageNumber = 1,
            CancellationToken cancellationToken = default)
        {
            var pagingValidationResult = await _pagingValidator.ValidateAsync(new Paging(pageSize, pageNumber), cancellationToken);
            if (!pagingValidationResult.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Errors = pagingValidationResult.Errors.Select(e => e.ErrorMessage).ToArray()
                });
            }

            var query = new GetUserOrdersQuery
            {
                Username = User.Identity.Name,
                PageSize = pageSize,
                PageNumber = pageNumber
            };

            var result = await _mediator.Send(query, cancellationToken);

            return Ok(new GetCurrentUserOrdersResponse
            {
                Orders = result.Orders,
            });
        }
    }
}
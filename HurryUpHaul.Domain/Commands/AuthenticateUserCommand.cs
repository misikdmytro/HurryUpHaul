using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using HurryUpHaul.Domain.Configuration;
using HurryUpHaul.Domain.Constants;
using HurryUpHaul.Domain.Handlers;

using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;


namespace HurryUpHaul.Domain.Commands
{
    public class AuthenticateUserCommand : IRequest<AuthenticateUserCommandResult>
    {
        public string Username { get; init; }
        public string Password { get; init; }
    }

    public class AuthenticateUserCommandResult
    {
        public bool Success { get; init; }
        public string Token { get; init; }
        public IEnumerable<string> Errors { get; init; }
    }

    internal class AuthenticateUserCommandHandler : BaseHandler<AuthenticateUserCommand, AuthenticateUserCommandResult>
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IOptionsMonitor<JwtSettings> _jwtOptions;

        public AuthenticateUserCommandHandler(UserManager<IdentityUser> userManager,
            IOptionsMonitor<JwtSettings> jwtSettings,
            ILogger<AuthenticateUserCommandHandler> logger) : base(logger)
        {
            _userManager = userManager;
            _jwtOptions = jwtSettings;
        }

        protected override async Task<AuthenticateUserCommandResult> HandleInternal(AuthenticateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return new AuthenticateUserCommandResult
                {
                    Success = false,
                    Errors = ["Invalid username or password."]
                };
            }

            var result = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!result)
            {
                return new AuthenticateUserCommandResult
                {
                    Success = false,
                    Errors = ["Invalid username or password."]
                };
            }

            var roles = await _userManager.GetRolesAsync(user);

            var token = GenerateToken(user, roles);
            return new AuthenticateUserCommandResult
            {
                Success = true,
                Token = token
            };
        }

        private string GenerateToken(IdentityUser user, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Name, user.UserName),
                new(JwtRegisteredClaimNames.Sub, user.Id)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimNames.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.CurrentValue.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.Add(_jwtOptions.CurrentValue.ExpiresIn);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.CurrentValue.Issuer,
                audience: _jwtOptions.CurrentValue.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
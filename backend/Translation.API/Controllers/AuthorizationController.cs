using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;
using System.Xml.Linq;
using Translation.Infrastructure.Identity;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace translation_app.Controllers
{
    [ApiController]
    [Route("connect")]
    public class AuthorizationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IOidcProvider _oidcrovider;

        public AuthorizationController(
            UserManager<IdentityUser> userManager,
            IOidcProvider oidcService)
        {
            _userManager = userManager;
            _oidcrovider = oidcService;
        }

        [HttpGet("~/connect/authorize")]
        [HttpPost("~/connect/authorize")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Authorize()
        {
            var request = _oidcrovider.GetServerRequest();

            var result = await _oidcrovider.AuthenticateAsync(IdentityConstants.ApplicationScheme);

            if (!result.Succeeded)
            {
                return _oidcrovider.Challenge(IdentityConstants.ApplicationScheme);
            }

            var user = await _userManager.GetUserAsync(result.Principal) ??
                 throw new InvalidOperationException("User not found.");

            var claims = new List<System.Security.Claims.Claim>
            {
                 new Claim("sub", user.Id),
                 new Claim("email", user.Email),
                 new Claim("name", user.UserName)
             };

            var identity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            foreach (var claim in identity.Claims)
                claim.SetDestinations(GetDestinations(claim));

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(request.GetScopes());

            return _oidcrovider.SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpPost("~/connect/token")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(OpenIddictResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OpenIddictResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Exchange()
        {
            var request = _oidcrovider.GetServerRequest();

            if (request.IsAuthorizationCodeGrantType())
            {
                var result = await _oidcrovider.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                return _oidcrovider.SignIn(result.Principal!, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            return BadRequest(new OpenIddictResponse { Error = Errors.UnsupportedGrantType });
        }

        private static IEnumerable<string> GetDestinations(Claim claim)
        {
            // Note: Destinations.AccessToken makes the claim available to your C# API
            // Destinations.IdentityToken makes it available to the Laravel PHP UI
            switch (claim.Type)
            {
                case Claims.Name:
                case Claims.Email:
                    yield return Destinations.AccessToken;
                    yield return Destinations.IdentityToken;
                    yield break;

                default:
                    yield return Destinations.AccessToken;
                    yield break;
            }
        }
    }

}


using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace translation_app.Controllers
{
    [ApiController]
    [Route("connect")]
    public class AuthorizationController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthorizationController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet("~/connect/authorize")]
        [HttpPost("~/connect/authorize")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Authorize()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            // 1. Authenticate using the Identity Cookie
            // IdentityConstants.ApplicationScheme matches your AddIdentity configuration
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

            // 2. If the user is not logged in, redirect them to the Laravel login page
            if (!result.Succeeded)
            {
                return Challenge(IdentityConstants.ApplicationScheme);
            }

            // 3. Retrieve the user from the database to ensure we have all claims
            var user = await _userManager.GetUserAsync(result.Principal) ??
                throw new InvalidOperationException("The user details cannot be retrieved.");

            // 4. Create the claims for the OpenIddict token
            string userId = await _userManager.GetUserIdAsync(user);
            string email = await _userManager.GetEmailAsync(user) ?? "";
            string name = await _userManager.GetUserNameAsync(user) ?? "";

            // 2. Build the list using the explicit System namespace
            var claims = new List<System.Security.Claims.Claim>
            {
                new Claim("sub", userId),
                new Claim("email", email),
                new Claim("name", name)
            };

            // 5. Build the ClaimsIdentity using the OpenIddict Server Scheme
            var identity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // 6. IMPORTANT: Map claims to destinations. 
            // Without this, Laravel will receive an empty token payload.
            foreach (var claim in identity.Claims)
            {
                claim.SetDestinations(GetDestinations(claim));
            }

            var principal = new ClaimsPrincipal(identity);

            // 7. Set the allowed scopes (openid, profile, email, etc.)
            principal.SetScopes(request.GetScopes());

            // 8. Return the SignIn result - this triggers the redirect back to Laravel with the 'code'
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpPost("~/connect/token")]
        [IgnoreAntiforgeryToken]
        [Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            // 1. Извлекаем OpenID Connect запрос из контекста
            var request = HttpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            // 2. Проверяем, что это обмен кода авторизации (Authorization Code Flow)
            if (request.IsAuthorizationCodeGrantType())
            {
                // 3. Извлекаем "личность" (ClaimsPrincipal), которую мы сохранили в коде ранее
                // Это делается через вызов AuthenticateAsync по схеме OpenIddict
                var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                // 4. Если всё ок, возвращаем SignIn. 
                // OpenIddict сам превратит это в JSON с Access Token
                return SignIn(result.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            return BadRequest(new OpenIddictResponse
            {
                Error = OpenIddictConstants.Errors.UnsupportedGrantType,
                ErrorDescription = "The specified grant type is not supported."
            });
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


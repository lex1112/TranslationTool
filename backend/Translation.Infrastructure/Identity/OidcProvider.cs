using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Translation.Infrastructure.Identity
{
    public class OidcProvider : IOidcProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OidcProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpContext Context => _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available.");

        public OpenIddictRequest GetServerRequest()
        {
            return Context.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException("The OIDC request cannot be retrieved.");
        }

        public async Task<AuthenticateResult> AuthenticateAsync(string scheme)
        {
            return await Context.AuthenticateAsync(scheme);
        }

        public IActionResult SignIn(ClaimsPrincipal principal, string scheme)
        {
            return new SignInResult(scheme, principal);
        }

        public IActionResult Challenge(string scheme)
        {
            return new ChallengeResult(scheme);
        }
    }

}

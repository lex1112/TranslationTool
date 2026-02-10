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
    public interface IOidcProvider
    {
        // Retrieves the OIDC request parsed by the middleware
        OpenIddictRequest GetServerRequest();

        // Authenticates the user based on a specific scheme (e.g., Identity Cookie)
        Task<AuthenticateResult> AuthenticateAsync(string scheme);

        // Wraps the SignIn logic
        IActionResult SignIn(ClaimsPrincipal principal, string scheme);

        // Wraps the Challenge (redirect to login) logic
        IActionResult Challenge(string scheme);
    }

}

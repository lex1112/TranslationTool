
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using Translation.Infrastructure.Identity;
using translation_app.Controllers;
using static OpenIddict.Abstractions.OpenIddictConstants;
using IdentityResult = Microsoft.AspNetCore.Identity.SignInResult;
using SignInResult = Microsoft.AspNetCore.Mvc.SignInResult;

namespace TranslationTool.API.Tests
{
    [TestFixture]
    public class AuthorizationControllerTests
    {
        private Mock<UserManager<IdentityUser>> _userManagerMock;
        private Mock<IOidcProvider> _oidcProviderMock;
        private AuthorizationController _controller;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);

            _oidcProviderMock = new Mock<IOidcProvider>();

            _controller = new AuthorizationController(
                _userManagerMock.Object,
                _oidcProviderMock.Object);
        }


        [Test]
        public async Task Authorize_WhenAuthenticationFails_ReturnsChallenge()
        {
            _oidcProviderMock
                .Setup(x => x.AuthenticateAsync(IdentityConstants.ApplicationScheme))
                .ReturnsAsync(AuthenticateResult.Fail("failed"));

            _oidcProviderMock
                .Setup(x => x.Challenge(IdentityConstants.ApplicationScheme))
                .Returns(new ChallengeResult());

            var result = await _controller.Authorize();

            Assert.That(result, Is.InstanceOf<ChallengeResult>());
        }

        [Test]
        public void Authorize_WhenUserNotFound_ThrowsInvalidOperationException()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity("test"));

            _oidcProviderMock
                .Setup(x => x.AuthenticateAsync(IdentityConstants.ApplicationScheme))
                .ReturnsAsync(AuthenticateResult.Success(
                    new AuthenticationTicket(principal, IdentityConstants.ApplicationScheme)));

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync((IdentityUser?)null);

            Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Authorize());
        }

        [Test]
        public async Task Authorize_WhenSuccessful_ReturnsSignInActionResult()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity("test"));

            var user = new IdentityUser
            {
                Id = "user-id",
                Email = "test@test.com",
                UserName = "testuser"
            };

            var request = new OpenIddictRequest();
            request.Scope = "openid email";

            _oidcProviderMock
                .Setup(x => x.GetServerRequest())
                .Returns(request);

            _oidcProviderMock
                .Setup(x => x.AuthenticateAsync(IdentityConstants.ApplicationScheme))
                .ReturnsAsync(AuthenticateResult.Success(
                    new AuthenticationTicket(principal, IdentityConstants.ApplicationScheme)));

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync(user);

            _oidcProviderMock
                .Setup(x => x.SignIn(
                    It.IsAny<ClaimsPrincipal>(),
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme))
                .Returns(new OkResult());

            var result = await _controller.Authorize();

            Assert.That(result, Is.InstanceOf<OkResult>());
        }

        [Test]
        public async Task Exchange_WhenAuthorizationCodeGrant_ReturnsSignIn()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity("test"));

            var request = new OpenIddictRequest
            {
                GrantType = OpenIddictConstants.GrantTypes.AuthorizationCode
            };

            _oidcProviderMock
                .Setup(x => x.GetServerRequest())
                .Returns(request);

            _oidcProviderMock
                .Setup(x => x.AuthenticateAsync(
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme))
                .ReturnsAsync(AuthenticateResult.Success(
                    new AuthenticationTicket(
                        principal,
                        OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)));

            _oidcProviderMock
                .Setup(x => x.SignIn(
                    principal,
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme))
                .Returns(new OkResult());

            var result = await _controller.Exchange();

            Assert.That(result, Is.InstanceOf<OkResult>());
        }

        [Test]
        public async Task Exchange_WhenUnsupportedGrantType_ReturnsBadRequest()
        {
            var request = new OpenIddictRequest
            {
                GrantType = "password"
            };

            _oidcProviderMock
                .Setup(x => x.GetServerRequest())
                .Returns(request);

            var result = await _controller.Exchange();

            var badRequest = result as BadRequestObjectResult;

            Assert.That(badRequest, Is.Not.Null);
            Assert.That(badRequest!.Value, Is.InstanceOf<OpenIddictResponse>());
        }


        [Test]
        public void GetDestinations_ForEmailClaim_ReturnsAccessAndIdentityToken()
        {
            var claim = new Claim(Claims.Email, "test@test.com");

            var destinations = InvokeGetDestinations(claim).ToList();

            Assert.That(destinations, Does.Contain(Destinations.AccessToken));
            Assert.That(destinations, Does.Contain(Destinations.IdentityToken));
        }

        [Test]
        public void GetDestinations_ForOtherClaim_ReturnsAccessTokenOnly()
        {
            var claim = new Claim("custom", "value");

            var destinations = InvokeGetDestinations(claim).ToList();

            Assert.That(destinations.Count, Is.EqualTo(1));
            Assert.That(destinations[0], Is.EqualTo(Destinations.AccessToken));
        }

        private static IEnumerable<string> InvokeGetDestinations(Claim claim)
        {
            var method = typeof(AuthorizationController)
                .GetMethod("GetDestinations",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Static)!;

            return (IEnumerable<string>)method.Invoke(null, new object[] { claim })!;
        }
    }
}




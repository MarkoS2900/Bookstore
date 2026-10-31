using Duende.IdentityServer;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Identity.Controllers;

public class AccountController(IIdentityServerInteractionService interaction, TestUserStore users, ILogger<AccountController> logger) : Controller
{
    [HttpGet]
    public IActionResult Login(string returnUrl)
    {
        logger.LogInformation("Login page requested.");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string returnUrl)
    {
        logger.LogInformation("Login attempt received for user {Username}.", username);

        var context = await interaction.GetAuthorizationContextAsync(returnUrl, HttpContext.RequestAborted);

        if (context == null)
        {
            logger.LogWarning("Login rejected because the authorization return URL is invalid.");
            return BadRequest("Invalid return URL.");
        }

        if (!users.ValidateCredentials(username, password))
        {
            logger.LogWarning("Login failed for user {Username} due to invalid credentials.", username);

            ModelState.AddModelError("", "Invalid username or password.");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        var user = users.FindByUsername(username);

        var isUser = new IdentityServerUser(user.SubjectId)
        {
            DisplayName = user.Username,
            AdditionalClaims = user.Claims
        };

        await HttpContext.SignInAsync(isUser);

        logger.LogInformation("User {Username} successfully signed in.", username);

        return Redirect(returnUrl);
    }
}

using Duende.IdentityServer;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Identity.Controllers;

public class AccountController : Controller
{
    private readonly IIdentityServerInteractionService _interaction;    
    private readonly TestUserStore _users;

    public AccountController(
        IIdentityServerInteractionService interaction,
        TestUserStore users)
    {
        _interaction = interaction;
        _users = users;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string returnUrl)
    {
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl, HttpContext.RequestAborted);

        if (context == null)
            return BadRequest("Invalid return URL.");

        if (!_users.ValidateCredentials(username, password))
        {
            ModelState.AddModelError("", "Invalid username or password.");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        var user = _users.FindByUsername(username);

        var isUser = new IdentityServerUser(user.SubjectId)
        {
            DisplayName = user.Username,
            AdditionalClaims = user.Claims
        };

        // Sign them in using the extension method
        await HttpContext.SignInAsync(isUser);

        return Redirect(returnUrl);
    }
}

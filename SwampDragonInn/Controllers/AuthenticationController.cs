using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Threading.Tasks;

namespace SwampDragonInn.Controllers
{
    [Route("authentication")]
    public class AuthenticationController : Controller
    {
        [HttpGet("challenge")]
        public IActionResult Challenge(string returnUrl = "/")
        {
            return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        // [HttpGet("signout")]
        // public async Task<IActionResult> SignOutUser()
        // {
        //     // Sign out locally
        //     await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //     await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

        //     // Build Cognito logout URL
        //     var config = HttpContext.RequestServices.GetService<IConfiguration>();
        //     var cognitoDomain = config?["Cognito:Authority"];
        //     var clientId = config?["Cognito:ClientId"];
        //     var request = HttpContext.Request;
        //     var baseUrl = $"{request.Scheme}://{request.Host}";
        //     var postLogoutRedirectUri = baseUrl + "/authentication/cognito-callback";
        //     var encodedRedirectUri = Uri.EscapeDataString(postLogoutRedirectUri);

        //     // Remove /oauth2 if present and ensure proper domain format
        //     if (!string.IsNullOrEmpty(cognitoDomain))
        //     {
        //         string domain = cognitoDomain.TrimEnd('/');
        //         if (domain.EndsWith("/oauth2"))
        //             domain = domain.Substring(0, domain.Length - "/oauth2".Length);

        //         var logger = HttpContext.RequestServices.GetService<ILogger<AuthenticationController>>();
        //         logger?.LogInformation("Original Cognito Domain: {Domain}", domain);

        //         var cognitoLogoutUrl = $"{domain}/logout?client_id={clientId}&redirect_uri={encodedRedirectUri}";

        //         logger?.LogInformation("Client ID: {ClientId}", clientId);
        //         logger?.LogInformation("Redirect URI (Encoded): {RedirectUri}", encodedRedirectUri);
        //         logger?.LogInformation("Redirect URI (Decoded): {RedirectUriDecoded}", postLogoutRedirectUri);
        //         logger?.LogInformation("Trying Cognito logout URL with redirect_uri: {LogoutUrl}", cognitoLogoutUrl);

        //         return Redirect(cognitoLogoutUrl);
        //     }

        //     // Fallback if Cognito configuration is missing
        //     return Redirect("/authentication/loggedout");
        // }

        [HttpGet("signout")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            return Redirect("/");
        }
        
        // [HttpGet("loggedout")]
        // public IActionResult LoggedOut()
        // {
        //     var logger = HttpContext.RequestServices.GetService<ILogger<AuthenticationController>>();
        //     logger?.LogInformation("LoggedOut endpoint was hit.");
        //     logger?.LogInformation("Request Host: {Host}", HttpContext.Request.Host);
        //     logger?.LogInformation("Request Path: {Path}", HttpContext.Request.Path);
        //     return Redirect("/");
        // }

        // [HttpGet("cognito-callback")]
        // public IActionResult CognitoCallback()
        // {
        //     var logger = HttpContext.RequestServices.GetService<ILogger<AuthenticationController>>();
        //     logger?.LogInformation("Cognito callback endpoint was hit.");
        //     logger?.LogInformation("Request Host: {Host}", HttpContext.Request.Host);
        //     logger?.LogInformation("Request Path: {Path}", HttpContext.Request.Path);
        //     logger?.LogInformation("Query String: {QueryString}", HttpContext.Request.QueryString);
        //     return Redirect("/");
        // }
    }
}
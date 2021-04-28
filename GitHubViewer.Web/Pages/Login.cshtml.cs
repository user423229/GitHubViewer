using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;
using GitHubViewer.Lib;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Octokit;

namespace GitHubViewer.Web.Pages
{
    /// <summary>
    /// Authenticates users with a given GitHub instance URI and access token.
    /// Note:
    /// For simplicity, this application does collect user's access token. Although it is more secure than passwords,
    /// for production environments the proper way would be to implement full OAuth web application flow,
    /// which redirects to GitHub login page for users to provide credentials.
    /// </summary>
    public class LoginPage : PageModel
    {
        private readonly ILogger<LoginPage> _logger;

        [FromQuery] public string ReturnUrl { get; init; }

        [BindProperty]
        [Required(ErrorMessage = "Please provide GitHub instance URI")]
        public string GitHubUri { get; init; }

        [BindProperty]
        [Required(ErrorMessage = "Please provide GitHub access token")]
        public string GitHubAccessToken { get; init; }

        public LoginPage(ILogger<LoginPage> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                Client client = new Client(new Uri(GitHubUri), GitHubAccessToken);
                User user = await client.GetUser();
                // now we're authenticated. We store the token and other info in HTTP session.
                HttpContext.Session.SetString(LoginMiddleware.UserKey, user.Login);
                HttpContext.Session.SetString(LoginMiddleware.UriKey, GitHubUri);
                HttpContext.Session.SetString(LoginMiddleware.AccessTokenKey, GitHubAccessToken);

                _logger.LogDebug("Logging in {0}", user.Login);

                return !string.IsNullOrEmpty(ReturnUrl) ? RedirectToPage(ReturnUrl) : Redirect("/");
            }
            catch (Exception e) when (e is AuthorizationException || e is UriFormatException ||
                                      e is HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, e.Message);
                return Page();
            }
        }
    }
}
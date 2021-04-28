using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GitHubViewer.Web.Pages
{
    public class LogoutPage : PageModel
    {
        private readonly ILogger<LoginPage> _logger;
        private readonly IOptions<SessionOptions> _sessionOptions;

        public LogoutPage(ILogger<LoginPage> logger, IOptions<SessionOptions> sessionOptions)
        {
            _logger = logger;
            _sessionOptions = sessionOptions;
        }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString(LoginMiddleware.UserKey) == null)
            {
                return Redirect("/"); // already logged out
            }

            _logger.LogDebug("Logging out {0}", HttpContext.Session.GetString(LoginMiddleware.UserKey));
            HttpContext.Session.Clear();
            Response.Cookies.Delete(_sessionOptions.Value.Cookie.Name!); // we're making sure to completely remove the session
            return Page();
        }
    }
}
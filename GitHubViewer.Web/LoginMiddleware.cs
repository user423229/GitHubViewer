using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace GitHubViewer.Web
{
    /// <summary>
    /// This middleware protects pages that require authorization (which are all pages unless they are marked as anonymous).
    /// If the user is currently not logged in, it redirects to login page.
    /// </summary>
    public class LoginMiddleware
    {
        // session keys
        public const string UserKey = "User";
        public const string UriKey = "Uri";
        public const string AccessTokenKey = "AccessToken";

        private readonly RequestDelegate _next;

        public LoginMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string requestedPage = context.GetEndpoint()?.Metadata.GetMetadata<PageRouteMetadata>()?.PageRoute;
            bool allowAnonymous = context.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() != null;
            
            // requested page exists, requires login and we're not logged in
            if (requestedPage != null && !allowAnonymous && context.Session.GetString(UserKey) == null)
            {
                context.Response.Redirect($"/Login?ReturnUrl={requestedPage}");
                return;
            }

            await _next(context);
        }
    }
}

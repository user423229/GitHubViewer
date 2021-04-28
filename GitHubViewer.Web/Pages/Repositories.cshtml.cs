using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHubViewer.Lib;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GitHubViewer.Web.Pages
{
    public class RepositoriesPage : PageModel
    {
        private const int MaxCommits = 10;

        public IList<RepositoryAndCommits> RepositoriesList { get; private set; }

        public async Task OnGet()
        {
            Client client = new Client(new Uri(HttpContext.Session.GetString(LoginMiddleware.UriKey)),
                HttpContext.Session.GetString(LoginMiddleware.AccessTokenKey));
            RepositoriesList = await client.GetRepositoriesAndCommits(MaxCommits);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GitHubViewer.Lib;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Sdk;

namespace GitHubViewer.Web.Tests
{
    ///
    /// Web integration tests.
    /// They utilize in-memory test web server provided by ASP.NET Core.
    ///
    /// For testing response content, we're doing a simple string match in HTML response.
    /// That could be improved by using additional libraries (like AngleSharp) to parse HTML response.
    /// 
    public class WebTests: IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public WebTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }
        
        private static string GetGitHubUriFromEnvironment() => 
            Environment.GetEnvironmentVariable("TEST_GITHUB_URI") ??
            throw new XunitException("TEST_GITHUB_URI environment variable not found");

        private static string GetGitHubAccessTokenFromEnvironment() =>
            Environment.GetEnvironmentVariable("TEST_GITHUB_ACCESS_TOKEN") ??
            throw new XunitException("TEST_GITHUB_ACCESS_TOKEN environment variable not found");
        
        private async Task<HttpResponseMessage> Login(string accessToken)
        {
            var formContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new("GitHubUri", GetGitHubUriFromEnvironment()),
                new("GitHubAccessToken", accessToken)
            });
            return await _client.PostAsync("/Login", formContent);
        }
        
        [Fact]
        public async Task NotFound()
        {
            var response = await _client.GetAsync("/non_existent_page");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task LoginInvalidCredentials()
        {
            var response = await Login("__invalid_access_token");
            string responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Bad credentials", responseContent);
        }
        
        [Fact]
        public async Task LoginValidCredentials()
        {
            var response = await Login(GetGitHubAccessTokenFromEnvironment());
            string responseContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("You are logged in", responseContent);
        }
        
        [Fact]
        public async Task RepositoriesNotLoggedIn()
        {
            var response = await _client.GetAsync("/Repositories");
            
            // should be redirected to login
            Assert.Equal("/Login", response.RequestMessage?.RequestUri?.AbsolutePath);
        }
        
        [Fact]
        public async Task Repositories()
        {
            await Login(GetGitHubAccessTokenFromEnvironment());
            var response = await _client.GetAsync("/Repositories");
            string responseContent = await response.Content.ReadAsStringAsync();
            
            Assert.Contains("Repositories accessible to", responseContent);
        }
    }
    
    ///
    /// This custom WebApplicationFactory disables CSRF checks to ease testing.
    /// 
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddRazorPages(options =>
                {
                    options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
                });
            });
        }
    }
}

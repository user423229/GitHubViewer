using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using Xunit;
using Xunit.Sdk;

namespace GitHubViewer.Lib.Tests
{
    public class ClientTests
    {
        private static string GetGitHubUriFromEnvironment() => 
            Environment.GetEnvironmentVariable("TEST_GITHUB_URI") ??
            throw new XunitException("TEST_GITHUB_URI environment variable not found");

        private static string GetGitHubAccessTokenFromEnvironment() =>
            Environment.GetEnvironmentVariable("TEST_GITHUB_ACCESS_TOKEN") ??
            throw new XunitException($"TEST_GITHUB_ACCESS_TOKEN environment variable not found");

        [Fact]
        public async Task GetUser()
        {
            Client client = new Client(new Uri(GetGitHubUriFromEnvironment()), GetGitHubAccessTokenFromEnvironment());
            User user = await client.GetUser();
            Assert.NotEmpty(user.Login);
        }

        [Fact]
        public async Task GetRepositoriesAndCommits()
        {
            const int maxCommits = 10;

            Client client = new Client(new Uri(GetGitHubUriFromEnvironment()), GetGitHubAccessTokenFromEnvironment());
            IList<RepositoryAndCommits> repositories = await client.GetRepositoriesAndCommits(maxCommits);

            foreach (RepositoryAndCommits repository in repositories)
            {
                Assert.InRange(repository.Commits.Count(), 0, maxCommits);
            }
        }

        [Fact]
        public async Task InvalidToken()
        {
            await Assert.ThrowsAsync<AuthorizationException>(async () =>
            {
                await new Client(new Uri(GetGitHubUriFromEnvironment()), "_invalidToken").GetUser();
            });
        }
    }
}
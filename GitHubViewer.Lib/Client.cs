using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace GitHubViewer.Lib
{
    public class Client
    {
        // Identification of this client presented to GitHub API
        private const string ClientApiIdentifier = "GitHubViewer";
        
        private readonly GitHubClient _client;

        /// <summary>
        /// Creates a new instance of the GitHub client.
        /// </summary>
        /// <param name="uri">GitHub instance URI.</param>
        /// <param name="token">Access token for GitHub API.</param>
        /// <exception cref="T:Octokit.AuthorizationException">Thrown if the given access token is not valid.</exception>
        /// <exception cref="T:Octokit.ApiException">Thrown when a general API error occurs.</exception>
        public Client(Uri uri, string token)
        {
            _client = new GitHubClient(new ProductHeaderValue(ClientApiIdentifier), uri)
            {
                Credentials = new Credentials(token)
            };
        }

        /// <summary>
        /// Returns a <see cref="T:Octokit.User" /> for the current authenticated user.
        /// </summary>
        /// <exception cref="T:Octokit.AuthorizationException">Thrown if the given access token is not valid.</exception>
        public async Task<User> GetUser()
        {
            return await _client.User.Current();
        }

        /// <summary>
        /// Returns all repositories accessible to the user, and the most recent commits of the default branch of each repository.
        /// </summary>
        /// <param name="maxCommits">Maximum number of commits per repository to return.</param>
        /// <exception cref="T:Octokit.AuthorizationException">Thrown if the given access token is not valid.</exception>
        /// <exception cref="T:Octokit.ApiException">Thrown when a general API error occurs.</exception>
        public async Task<IList<RepositoryAndCommits>> GetRepositoriesAndCommits(int maxCommits)
        {
            const int maxConcurrency = 5;

            IReadOnlyList<Repository> repositories = await _client.Repository.GetAllForCurrent(new RepositoryRequest
            {
                Affiliation = RepositoryAffiliation.All
            });

            // we want to preserve the order of repositories as returned by the API. After completion of tasks
            // that fetch commits for each repository we will fill elements of this list with appropriate commits. 
            IList<RepositoryAndCommits> repositoriesResult = repositories.Select(repository => new RepositoryAndCommits
            {
                Repository = repository
            }).ToList();

            // local function that fetches commits for a given repository
            async Task<(Repository, IReadOnlyList<GitHubCommit>)> GetCommits(Repository repository)
            {
                IReadOnlyList<GitHubCommit> commits;
                try
                {
                    commits = await _client.Repository.Commit.GetAll(repository.Id,
                        new ApiOptions {PageSize = maxCommits, PageCount = 1});
                }
                catch (ApiException e)
                {
                    // A repository can contain no commits. Looks like a comparison to a string in
                    // exception message is the only way to detect this
                    if (e.Message == "Git Repository is empty.")
                    {
                        commits = new List<GitHubCommit>();
                    }
                    else
                    {
                        throw;
                    }
                }

                return (repository, commits);
            }

            // we are fetching commits for many repositories concurrently, up to the given limit
            var commitsTasks = AsyncUtils.RunForEach(repositories, GetCommits, maxConcurrency);
            await foreach (var (repository, commits) in commitsTasks)
            {
                repositoriesResult.Single(r => r.Repository.Id == repository.Id).Commits = commits;
            }

            return repositoriesResult;
        }
    }
}
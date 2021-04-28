using System.Collections.Generic;
using Octokit;

namespace GitHubViewer.Lib
{
    public class RepositoryAndCommits
    {
        public Repository Repository { get; set; }
        public IEnumerable<GitHubCommit> Commits { get; set; }
    }
}
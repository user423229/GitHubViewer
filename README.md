# GitHub Viewer

This application allows users to view their GitHub repositories and most recent commits of each repository.

## Building
This is a .NET 5 ASP.NET Core application.
To build the application and run tests, use regular .NET 5 SDK commands or an IDE.

**Note**: when running tests (from an IDE, command line shell or CI/CD environment), make sure to configure environvent variables 
`TEST_GITHUB_URI` and `TEST_GITHUB_ACCESS_TOKEN` with appropriate values. This GitHub repository has these values already configured as encrypted secrets,
which are then used in CI/CD GitHub Actions setup.

## Prebuilt artifacts for 64-bit Windows 10
This repository has GitHub Actions setup that builds 64-bit Windows 10 binaries. 
They are built as standalone executables, so they don't have a dependency on .NET 5 runtime.
To access the binaries download `GitHubViewer.zip` from Releases.

## Running the application
- Extract the downloaded `GitHubViewer.zip` file.
- Run `GitHubViewer.exe`. If running from command line, make sure to first `cd` into directory containing the `.exe` file.
- The application is available under http://localhost:5000.

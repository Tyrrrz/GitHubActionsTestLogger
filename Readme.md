# GitHubActionsTestLogger

[![Build](https://github.com/Tyrrrz/GitHubActionsTestLogger/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/GitHubActionsTestLogger/actions)
[![Version](https://img.shields.io/nuget/v/GitHubActionsTestLogger.svg)](https://nuget.org/packages/GitHubActionsTestLogger)
[![Downloads](https://img.shields.io/nuget/dt/GitHubActionsTestLogger.svg)](https://nuget.org/packages/GitHubActionsTestLogger)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)

Custom test logger for .NET that writes output in a format that GitHub Actions understands. When using this logger, failed tests will show up in annotations as well as in the list of failed checks.

## Download

- [NuGet](https://nuget.org/packages/GitHubActionsTestLogger): `dotnet add package GitHubActionsTestLogger`

## Screenshots

![diff](./.screenshots/diff.png)
![annotations](./.screenshots/annotations.png)

## Usage

1. Install the NuGet package in the test project

2. Update your workflow so tests are ran with custom logger:

```yaml
steps:
  # ...

  # Specify 'GitHubActions' as custom logger
  - name: Build & test
    run: dotnet test -c Release -l GitHubActions
```

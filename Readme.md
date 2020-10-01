# GitHubActionsTestLogger

[![Build](https://github.com/Tyrrrz/GitHubActionsTestLogger/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/GitHubActionsTestLogger/actions)
[![Version](https://img.shields.io/nuget/v/GitHubActionsTestLogger.svg)](https://nuget.org/packages/GitHubActionsTestLogger)
[![Downloads](https://img.shields.io/nuget/dt/GitHubActionsTestLogger.svg)](https://nuget.org/packages/GitHubActionsTestLogger)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)

Custom test logger for `dotnet test` that writes output in a structured format which is understood by GitHub Actions. When using this logger, failed tests show up in diffs as well as in the list of annotations.

## Download

- [NuGet](https://nuget.org/packages/GitHubActionsTestLogger): `dotnet add package GitHubActionsTestLogger`

## Screenshots

Failed tests are highlighted in diffs:

![diff](./.screenshots/diff.png)

Failed tests are listed in annotations:

![annotations](./.screenshots/annotations.png)

## Usage

Prerequisites:

- .NET SDK v3.0 or higher
- Latest version of `Microsoft.NET.Test.Sdk` referenced in your test project

Setup:

1. Install `GitHubActionsTestLogger` in your test project via NuGet

2. Update your workflow by adding a logger option to `dotnet test`:

```yaml
steps:
  # ...

  - name: Build & test
    run: dotnet test --logger GitHubActions
```

### Ignore warnings

By default, GitHubActionsTestLogger produces warnings for tests that have neither failed nor succeeded (e.g. skipped). If you don't want that, you can disable it with a parameter:

```sh
dotnet test --logger "GitHubActions;report-warnings=false"
```

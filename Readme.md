# GitHubActionsTestLogger

[![Build](https://github.com/Tyrrrz/GitHubActionsTestLogger/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/GitHubActionsTestLogger/actions)
[![Coverage](https://codecov.io/gh/Tyrrrz/GitHubActionsTestLogger/branch/master/graph/badge.svg)](https://codecov.io/gh/Tyrrrz/GitHubActionsTestLogger)
[![Version](https://img.shields.io/nuget/v/GitHubActionsTestLogger.svg)](https://nuget.org/packages/GitHubActionsTestLogger)
[![Downloads](https://img.shields.io/nuget/dt/GitHubActionsTestLogger.svg)](https://nuget.org/packages/GitHubActionsTestLogger)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)

✅ **Project status: active**.

Custom logger for `dotnet test` that reports test results in a structured format that GitHub Actions understands.
When using this logger, failed tests are listed in workflow annotations and highlighted in code diffs.

## Download

📦 [NuGet](https://nuget.org/packages/GitHubActionsTestLogger): `dotnet add package GitHubActionsTestLogger`

## Screenshots

![diff](./.screenshots/diff.png)

## Usage

### Installation

To use GitHubActionsTestLogger, follow these steps:

1. Install `GitHubActionsTestLogger` package in your test project
2. Install `Microsoft.NET.Test.Sdk` package in your test project (or update to latest)
3. Modify your GitHub Actions workflow file by adding `--logger GitHubActions` to `dotnet test`:

```yaml
name: CI
on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v2.3.3

      - name: Install .NET
        uses: actions/setup-dotnet@v1.7.2
        with:
          # Note: version 3.0.x or higher is required!
          dotnet-version: 5.0.x

      - name: Build & test
        # Specify custom logger to use via the `--logger` option
        run: dotnet test --configuration Release --logger GitHubActions
```

⚠️ Ensure that you are running your tests with **.NET SDK v3.0 or higher**.
Older versions of the SDK don't support custom test loggers added via NuGet packages.

⚠️ Ensure that your test project references `Microsoft.NET.Test.Sdk` version `16.8.0` or higher.
Older versions of this package may not work properly with custom test loggers.

### Options

By default, GitHubActionsTestLogger produces warnings for tests that have neither failed nor succeeded (i.e. skipped or inconclusive).
If that's not desirable, you can disable this behavior with a switch:

```sh
dotnet test --logger "GitHubActions;report-warnings=false"
```

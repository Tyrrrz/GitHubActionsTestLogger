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
        # Specify custom logger here
        run: dotnet test --configuration Release --logger GitHubActions
```

⚠️ Ensure that you are running your tests with **.NET SDK v3.0 or higher**.
Older versions of the SDK don't support custom test loggers added via NuGet packages.

⚠️ Ensure that your test project references `Microsoft.NET.Test.Sdk` version `16.8.0` or higher.
Older versions of this package may not work properly with custom test loggers.

### Options

GitHubActionsTestLogger has a few options that you can override to customize its behavior.
In order to pass an option to the logger, include it as a parameter to the `--logger` option like so:

```sh
dotnet test --logger "GitHubActions;option1=foo;option2=bar"
```

The following options are supported:

#### `format`

Specifies the format used when logging test results to the console.

The following replacement tokens are available:

- `$test` -- replaced with the display name of the test
- `$outcome` -- replaced with the error message (in case of an exception) or the outcome of the test
- `$traits.TRAIT_NAME` -- replaced with the value of the trait named `TRAIT_NAME`

Default: `$test: $outcome`.

Examples:

- `$test: $outcome` -> `MyTests.Test1: AssertionException: Expected 'true' but found 'false'`
- `[$traits.Category] $test: $outcome` -> `[UI Tests] MyTests.Test1: AssertionException: Expected 'true' but found 'false'`

#### `report-warnings`

Specifies whether to report warnings for tests that have neither failed nor succeeded (i.e. skipped or inconclusive).
If disabled, only failed tests will be logged and all other tests will be ignored.
Can be either `true` or `false`.

Default: `true`.

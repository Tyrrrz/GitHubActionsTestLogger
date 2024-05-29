# GitHub Actions Test Logger

[![Status](https://img.shields.io/badge/status-maintenance-ffd700.svg)](https://github.com/Tyrrrz/.github/blob/master/docs/project-status.md)
[![Made in Ukraine](https://img.shields.io/badge/made_in-ukraine-ffd700.svg?labelColor=0057b7)](https://tyrrrz.me/ukraine)
[![Build](https://img.shields.io/github/actions/workflow/status/Tyrrrz/GitHubActionsTestLogger/main.yml?branch=master)](https://github.com/Tyrrrz/GitHubActionsTestLogger/actions)
[![Coverage](https://img.shields.io/codecov/c/github/Tyrrrz/GitHubActionsTestLogger/master)](https://codecov.io/gh/Tyrrrz/GitHubActionsTestLogger)
[![Version](https://img.shields.io/nuget/v/GitHubActionsTestLogger.svg)](https://nuget.org/packages/GitHubActionsTestLogger)
[![Downloads](https://img.shields.io/nuget/dt/GitHubActionsTestLogger.svg)](https://nuget.org/packages/GitHubActionsTestLogger)
[![Discord](https://img.shields.io/discord/869237470565392384?label=discord)](https://discord.gg/2SUWKFnHSm)
[![Fuck Russia](https://img.shields.io/badge/fuck-russia-e4181c.svg?labelColor=000000)](https://twitter.com/tyrrrz/status/1495972128977571848)

<table>
    <tr>
        <td width="99999" align="center">Development of this project is entirely funded by the community. <b><a href="https://tyrrrz.me/donate">Consider donating to support!</a></b></td>
    </tr>
</table>

<p align="center">
    <img src="favicon.png" alt="Icon" />
</p>

**GitHub Actions Test Logger** is a custom logger for `dotnet test` that integrates with GitHub Actions.
When using this logger, failed tests are listed in job annotations and highlighted in code diffs.
Additionally, this logger also generates a job summary that contains detailed information about the executed test run.

## Terms of use<sup>[[?]](https://github.com/Tyrrrz/.github/blob/master/docs/why-so-political.md)</sup>

By using this project or its source code, for any purpose and in any shape or form, you grant your **implicit agreement** to all the following statements:

- You **condemn Russia and its military aggression against Ukraine**
- You **recognize that Russia is an occupant that unlawfully invaded a sovereign state**
- You **support Ukraine's territorial integrity, including its claims over temporarily occupied territories of Crimea and Donbas**
- You **reject false narratives perpetuated by Russian state propaganda**

To learn more about the war and how you can help, [click here](https://tyrrrz.me/ukraine). Glory to Ukraine! 🇺🇦

## Install

- 📦 [NuGet](https://nuget.org/packages/GitHubActionsTestLogger): `dotnet add package GitHubActionsTestLogger`

## Screenshots

![annotations](.assets/annotations.png)
![summary](.assets/summary.png)

## Usage

To use **GitHub Actions Test Logger**, install it in your test project and modify your GitHub Actions workflow by adding `--logger GitHubActions` to `dotnet test`:

```yaml
name: main
on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Install .NET
        uses: actions/setup-dotnet@v4

      - name: Build & test
        run: dotnet test --configuration Release --logger GitHubActions
```

By default, the logger will only report failed tests in the job summary and annotations.
If you want the summary to include detailed information about passed and skipped tests as well, update the workflow as follows:

```yaml
jobs:
  build:
    runs-on: ubuntu-latest

    steps:
      # ...

      - name: Build & test
        run: >
          dotnet test
          --configuration Release
          --logger "GitHubActions;summary.includePassedTests=true;summary.includeSkippedTests=true"
```

> **Important**:
> Ensure that your test project references the latest version of **Microsoft.NET.Test.Sdk**.
> Older versions of this package may not be compatible with the logger.

> **Important**:
> If you are using **.NET SDK v2.2 or lower**, you need to set the `<CopyLocalLockFileAssemblies>` property to `true` in your test project.
> [Learn more](https://github.com/Tyrrrz/GitHubActionsTestLogger/issues/5#issuecomment-648431667).

### Collecting source information

**GitHub Actions Test Logger** can leverage source information to link reported test results to the locations in the source code where the corresponding tests are defined.
By default, `dotnet test` does not collect source information, so the logger relies on stack traces to extract it manually.
This approach only works for failed tests, and even then may not always be fully accurate.

To instruct the runner to collect source information, add the `RunConfiguration.CollectSourceInformation=true` argument to the command as shown below:

```yml
jobs:
  build:
    runs-on: ubuntu-latest

    steps:
      # ...

      - name: Build & test
        # Note that the space after the last double dash (--) is intentional
        run: >
          dotnet test
          --configuration Release
          --logger GitHubActions
          --
          RunConfiguration.CollectSourceInformation=true
```

> **Note**:
> This option can also be enabled by setting the corresponding property in a `.runsettings` file instead.
> [Learn more](https://learn.microsoft.com/en-us/visualstudio/test/configure-unit-tests-by-using-a-dot-runsettings-file).

> **Warning**:
> Source information collection is not supported on legacy .NET Framework.

### Customizing behavior

When running `dotnet test`, you can customize the logger's behavior by passing additional options:

```yml
jobs:
  build:
    runs-on: ubuntu-latest

    steps:
      # ...

      - name: Build & test
        run: >
          dotnet test
          --configuration Release
          --logger "GitHubActions;annotations.titleFormat=@test;annotations.messageFormat=@error"
```

#### Custom annotation title

Use the `annotations.titleFormat` option to specify the annotation title format used for reporting test failures.

The following replacement tokens are available:

- `@test` — replaced with the display name of the test
- `@traits.TRAIT_NAME` — replaced with the value of the trait named `TRAIT_NAME`
- `@error` — replaced with the error message
- `@trace` — replaced with the stack trace
- `@framework` — replaced with the target framework

**Default**: `@test`.

**Examples**:

- `@test` → `MyTests.Test1`
- `[@traits.Category] @test` → `[UI Tests] MyTests.Test1`
- `@test (@framework)` → `MyTests.Test1 (.NETCoreApp,Version=v6.0)`

#### Custom annotation message

Use the `annotations.messageFormat` option to specify the annotation message format used for reporting test failures.
Supports the same replacement tokens as [`annotations.titleFormat`](#custom-annotation-title).

**Default**: `@error`.

**Examples**:

- `@error` → `AssertionException: Expected 'true' but found 'false'`
- `@error\n@trace` → `AssertionException: Expected 'true' but found 'false'`, followed by stacktrace on the next line

#### Include passed tests in summary

Use the `summary.includePassedTests` option to specify whether passed tests should be included in the summary.
If you want to link passed tests to their corresponding source definitions, make sure to also enable [source information collection](#collecting-source-information).

**Default**: `false`.

> **Warning**:
> If your test suite is really large, enabling this option may cause the summary to exceed the [maximum allowed size](https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions#step-isolation-and-limits).

#### Include skipped tests in summary

Use the `summary.includeSkippedTests` option to specify whether skipped tests should be included in the summary.
If you want to link skipped tests to their corresponding source definitions, make sure to also enable [source information collection](#collecting-source-information).

**Default**: `false`.

> **Warning**:
> If your test suite is really large, enabling this option may cause the summary to exceed the [maximum allowed size](https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions#step-isolation-and-limits).

#### Include not found tests in summary

Use the `summary.includeNotFoundTests` option to specify whether empty test assemblies should be included in the summary.

Using [test filters](https://learn.microsoft.com/en-us/dotnet/core/testing/selective-unit-tests) might result in some test assemblies not yielding any matching tests.
This might be done on purpose in which case reporting these may not be helpful. 

**Default**: `true`.

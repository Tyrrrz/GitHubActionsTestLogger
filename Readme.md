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

**GitHub Actions Test Logger** is an extension for VSTest and MTP that reports test results via GitHub Actions.
It lists failed tests in job annotations, highlights them in code diffs, and produces a detailed job summary with information about the executed test run.

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

**GitHub Actions Test Logger** is available for both the classic **VSTest** test runner and the newer **Microsoft.Testing.Platform**.

### [Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro)

Simply install the package in your test project.
The provided test reporter will automatically be detected and enabled when running in a GitHub Actions environment.

### [VSTest](https://github.com/microsoft/vstest)

Install the package in your test project and specify the logger when running `dotnet test`:

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

> **Important**:
> Ensure that your test project references the latest version of **Microsoft.NET.Test.Sdk**.
> Older versions of this package may not be compatible with the logger.

> **Important**:
> If you are using **.NET SDK v2.2 or lower**, you need to [set the `<CopyLocalLockFileAssemblies>` property to `true` in your test project](https://github.com/Tyrrrz/GitHubActionsTestLogger/issues/5#issuecomment-648431667).

#### Collecting source information

**GitHub Actions Test Logger** can leverage source information to link reported test results to the locations in the source code where the corresponding tests are defined.
By default, VSTest does not collect source information, so the logger relies on stack traces to extract it manually.
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
> This option can also be enabled by setting the corresponding property in a [`.runsettings` file](https://learn.microsoft.com/en-us/visualstudio/test/configure-unit-tests-by-using-a-dot-runsettings-file) instead.

> **Warning**:
> Source information collection may not work properly with legacy .NET Framework.

### Customizing behavior

When running `dotnet test`, you can customize the logger's behavior by passing additional options on the command line:

**Microsoft.Testing.Platform**:

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
          --
          --report-github
          --report-github-annotations-title=@test
          --report-github-annotations-message=@error
```

**VSTest**:

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
          --logger "GitHubActions;annotations-title=@test;annotations-message=@error"
```

#### Custom annotation title

Use the `[--report-github-]annotations-title` option to specify the annotation title format used for reporting test failures.

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

Use the `[--report-github-]annotations-message` option to specify the annotation message format used for reporting test failures.
Supports the same replacement tokens as [`annotations-title-format`](#custom-annotation-title).

**Default**: `@error`.

**Examples**:

- `@error` → `AssertionException: Expected 'true' but found 'false'`
- `@error\n@trace` → `AssertionException: Expected 'true' but found 'false'`, followed by stacktrace on the next line

#### Allow empty test summaries

Use the `[--report-github-]summary-allow-empty` option to specify whether empty test runs should be included in the summary, for example as a result of using [test filters](https://learn.microsoft.com/dotnet/core/testing/selective-unit-tests).

**Default**: `false`.

#### Include passed tests in the summary

Use the `[--report-github-]summary-include-passed` option to specify whether passed tests should be included in the summary.
If you want to link passed tests to their corresponding source definitions, make sure to also enable [source information collection](#collecting-source-information).

**Default**: `true`.

> **Warning**:
> If your test suite is really large, enabling this option may cause the summary to exceed the [maximum allowed size](https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions#step-isolation-and-limits).

#### Include skipped tests in the summary

Use the `[--report-github-]summary-include-skipped` option to specify whether skipped tests should be included in the summary.
If you want to link skipped tests to their corresponding source definitions, make sure to also enable [source information collection](#collecting-source-information).

**Default**: `true`.

> **Warning**:
> If your test suite is really large, enabling this option may cause the summary to exceed the [maximum allowed size](https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions#step-isolation-and-limits).


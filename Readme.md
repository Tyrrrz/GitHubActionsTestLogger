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

**GitHub Actions Test Logger** is an extension for **VSTest** and **Microsoft.Testing.Platform** that reports test results to GitHub Actions.
It lists failed tests in job annotations, highlights them in code diffs, and produces detailed job summaries about the executed test runs.

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

Install the package in your test project and the provided test reporter will be detected and registered automatically.

The reporter is enabled by default when running in a GitHub Actions environment.
You can also enable it manually by adding the `--report-github` option when running tests:

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
        # Can also use with `dotnet run` or `dotnet exec`, depending on your setup.
        # For .NET 9 and lower, add an extra empty double dash sequence (--) before reporter options.
        run: dotnet test --configuration Release --report-github
```

> [!IMPORTANT]
> If you are using **Microsoft.Testing.Platform** with **xUnit v3**, then make sure to replace the `xunit.v3` package reference with [`xunit.v3.mtp-v2`](https://nuget.org/packages/xunit.v3.mtp-v2).
> The base `xunit.v3` package relies on MTP v1, which is incompatible with this extension.

> [!IMPORTANT]
> The extension has a peer dependency on the [`Microsoft.Testing.Platform`](https://nuget.org/packages/Microsoft.Testing.Platform) package when used in this mode.
> It is **highly recommended** to install the latest version of this package in your test project to ensure compatibility.

> [!WARNING]
> When used with **Microsoft.Testing.Platform**, do not mark the package reference to `GitHubActionsTestLogger` as private or exclude it from the build output.
> If you are upgrading from older versions of the extension, make sure to remove `PrivateAssets="all"` from the package reference.

### [VSTest](https://github.com/microsoft/vstest)

Install the package in your test project and enable the reporter by adding the `--logger GitHubActions` option when running tests:

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

> [!IMPORTANT]
> If you are using **VSTest** with **xUnit v3**, then make sure to replace the `xunit.v3` package reference with [`xunit.v3.mtp-off`](https://nuget.org/packages/xunit.v3.mtp-off).
> The base `xunit.v3` package has a built-in dependency on MTP, which may cause conflicts with this extension.

> [!IMPORTANT]
> The extension has a peer dependency on the [`Microsoft.NET.Test.Sdk`](https://nuget.org/packages/Microsoft.NET.Test.Sdk) package when used in this mode.
> It is **highly recommended** to install the latest version of this package in your test project to ensure compatibility.

> [!IMPORTANT]
> If you are using **.NET SDK v2.2 or lower**, you need to [set the `<CopyLocalLockFileAssemblies>` property to `true` in your test project](https://github.com/Tyrrrz/GitHubActionsTestLogger/issues/5#issuecomment-648431667).

> [!NOTE]
> When used with **VSTest**, the package reference to `GitHubActionsTestLogger` can be marked as private by adding `PrivateAssets="all"` to the package reference.

#### Collecting source information

**GitHub Actions Test Logger** can leverage source information to link reported test results to the locations in the source code where the corresponding tests are defined.
By default, **VSTest** does not collect source information, so the extension relies on stack traces to extract it manually.
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

> [!NOTE]
> This option can also be enabled by setting the corresponding property in a [`.runsettings` file](https://learn.microsoft.com/visualstudio/test/configure-unit-tests-by-using-a-dot-runsettings-file) instead.

> [!WARNING]
> Source information collection may not work properly with the legacy .NET Framework.

### Customizing behavior

You can customize the behavior of **GitHub Actions Test Logger** by passing additional options when running tests.
The format of these options differs slightly between **Microsoft.Testing.Platform** and **VSTest**.

With **Microsoft.Testing.Platform**, the options are prefixed with `--report-github-` and can be specified as separate arguments on the command line:

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
          --report-github
          --report-github-annotations-title @test
          --report-github-annotations-message @error
```

With **VSTest**, the options don't have a prefix and are specified as part of the reporter configuration string, delimited by semicolons:

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

- `@test` — replaced with the display name of the test.
- `@error` — replaced with the error message.
- `@trace` — replaced with the stack trace.
- `@framework` — replaced with the target framework.

**Default**: `@test`.

**Examples**:

- `@test` → `MyTests.Test1`
- `@test (@framework)` → `MyTests.Test1 (.NETCoreApp,Version=v6.0)`

#### Custom annotation message

Use the `[--report-github-]annotations-message` option to specify the annotation message format used for reporting test failures.
Supports the same replacement tokens as the [title format](#custom-annotation-title).

**Default**: `@error`.

**Examples**:

- `@error` → `AssertionException: Expected 'true' but found 'false'`
- `@error\n@trace` → `AssertionException: Expected 'true' but found 'false'`, followed by stacktrace on the next line

#### Allow empty test summaries

Use the `[--report-github-]summary-allow-empty` option to specify whether empty test runs should be included in the summary.

**Default**: `false`.

#### Include passed tests in the summary

Use the `[--report-github-]summary-include-passed` option to specify whether passed tests should be included in the summary.

**Default**: `true`.

> [!WARNING]
> If your test suite is really large, enabling this option may cause the summary to exceed the [maximum allowed size](https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions#step-isolation-and-limits).

#### Include skipped tests in the summary

Use the `[--report-github-]summary-include-skipped` option to specify whether skipped tests should be included in the summary.

**Default**: `true`.

> [!WARNING]
> If your test suite is really large, enabling this option may cause the summary to exceed the [maximum allowed size](https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions#step-isolation-and-limits).


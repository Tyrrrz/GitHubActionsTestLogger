# GitHub Actions Test Logger Demo (`Microsoft.Testing.Platform`)

To run the demo tests, use the following command:

```console
$ dotnet test -p:IsTestProject=true
```

In order for the reporter to work, it needs to think it's running within a GitHub Actions environment.
You can simulate this locally by setting the `GITHUB_ACTIONS` environment variable to `true`:

```powershell
$env:GITHUB_ACTIONS="true"
```

To produce a test summary, provide the output file path by setting the `GITHUB_STEP_SUMMARY` environment variable:

```powershell
$env:GITHUB_STEP_SUMMARY="./test-summary.md"
```

# GitHub Actions Test Logger Demo (`Microsoft.Testing.Platform`)

To run the demo tests, use the following command:

```console
$ dotnet test -p:IsTestProject=true --report-github
```

To produce a test summary, provide the output file path by setting the `GITHUB_STEP_SUMMARY` environment variable:

```powershell
$env:GITHUB_STEP_SUMMARY="./test-summary.md"
```

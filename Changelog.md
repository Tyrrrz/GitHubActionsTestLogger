### v1.1.2 (26-Oct-2020)

- Fixed an issue where source information was sometimes not properly extracted from tests implemented as async methods.

### v1.1.1 (01-Oct-2020)

- Added a warning message when using this logger outside of GitHub Actions.

### v1.1 (27-Apr-2020)

- Added a switch to disable warnings which are reported if tests were ignored or skipped. Use `--logger "GitHubActions;report-warnings=false"` if you want to do that.
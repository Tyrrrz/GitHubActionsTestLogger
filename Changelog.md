### v1.4 (18-Apr-2022)

- Added rudimentary support for GitHub Job Summaries (also known as Action Summaries or Step Summaries). Note that this feature is still in private preview and may not be available for your repository. See readme for more information.

### v1.3 (21-Feb-2022)

- Improved formatting of GitHub workflow commands. Newlines are now properly escaped, allowing multiline error messages to be shown inside annotations.
- Changed default annotation message format from `$name: $outcome` to just `$outcome`. Test name, which was previously included, is now displayed as annotation title instead.
- Marked package as development dependency.

### v1.2 (22-Feb-2021)

- Added a `format` option that allows customizing message format, with which the test results are reported. Refer to the readme for more information on how to use it.

### v1.1.2 (26-Oct-2020)

- Fixed an issue where source information was sometimes not properly extracted from tests implemented as async methods.

### v1.1.1 (01-Oct-2020)

- Added a warning message when using this logger outside of GitHub Actions.

### v1.1 (27-Apr-2020)

- Added a switch to disable warnings which are reported if tests were ignored or skipped. Use `--logger "GitHubActions;report-warnings=false"` if you want to do that.

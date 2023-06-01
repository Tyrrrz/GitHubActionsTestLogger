# Changelog

## v2.3.2 (01-Jun-2023)

- Changed the special character used to identify replacement tokens inside `annotations.titleFormat` and `annotations.messageFormat` from `$` to `@`. The previous character (`$`) had special meaning inside POSIX shells and required escaping, which resulted in non-uniform syntax across different platforms. The new character (`@`) does not require escaping. For backwards compatibility, the logger will still recognize the `$` character as a replacement token identifier, but it is recommended to switch to the new character (`@`) when possible.

## v2.3.1 (31-May-2023)

- Fixed minor spacing issues in the test summary.

## v2.3 (25-May-2023)

- Reverted some of the test summary layout changes introduced in v2.2 to restore a more compact view of the test results. This should make the summary more accessible for repositories that have a large number of test suites.

## v2.2.1 (18-May-2023)

- Fixed an issue where the test summary was not properly rendered when it was preceded by a different summary that ended in an HTML tag.
- Fixed minor layout-related issues in the test summary.

## v2.2 (18-May-2023)

- Reworked the test summary layout:
  - Overall test stats are no longer hidden by an expander.
  - Tests are now grouped by their defining type.
  - Other minor visual improvements.
- Fixed an issue where exception messages rendered under failed tests in the test summary broke formatting if they contained indented lines.

## v2.1 (09-May-2023)

- Added `summary.includePassedTests` and `summary.includeSkippedTests` options to extend the test summary with the list of passed and skipped tests, respectively. These options are set to `false` by default to match the previous behavior.
- Fixed an issue where the test summary had invalid links to test definitions when running with `ContinuousIntegrationBuild` MSBuild property enabled.

## v2.0.2 (27-Apr-2023)

- Improved execution performance of the logger when running multiple test suites in parallel.

## v2.0.1 (10-Jun-2022)

- Fixed an issue where the test summary showed incorrect value under the "Elapsed" column. The value shown previously was a sum of all test durations, which didn't account for parallelization.

## v2.0 (29-May-2022)

- Test summary now only displays failed tests instead of all executed tests. Displaying all tests made the summary very hard to navigate and caused severe performance issues in repositories with large test suites.
- Failed tests in test summary now link to the file and the exact code line where the error happened. This link is static and is bound to the commit hash that triggered the workflow.
- Redesigned the test summary to be more visually appealing and easier to read.
- Removed the `report-warnings` option. Skipped tests are no longer reported in annotations. The new default (and only) behavior is equivalent to setting `report-warnings=false` in the previous versions of the logger.
- Renamed the `format` option to `annotations.titleFormat` to better reflect its purpose. This option can be used to specify the format of the annotation title (the header of the alert box shown in diffs and job overviews).
- Added the `annotations.messageFormat` option that can be used to specify the format of the annotation message (the body of the alert box shown in diffs and job overviews).
- Added `$framework` replacement token that can be used in `annotations.titleFormat` and `annotations.messageFormat` to display the target framework of the associated test.

## v1.4.1 (19-Apr-2022)

- Improved test summary formatting and layout.

## v1.4 (18-Apr-2022)

- Added rudimentary support for GitHub Job Summaries (also known as Action Summaries or Step Summaries). Note that this feature is still in private preview and may not be available for your repository. See readme for more information.

## v1.3 (21-Feb-2022)

- Improved formatting of GitHub workflow commands. Newlines are now properly escaped, allowing multiline error messages to be shown inside annotations.
- Changed default annotation message format from `$name: $outcome` to just `$outcome`. Test name, which was previously included, is now displayed as annotation title instead.
- Marked package as development dependency.

## v1.2 (22-Feb-2021)

- Added a `format` option that allows customizing message format, with which the test results are reported. Refer to the readme for more information on how to use it.

## v1.1.2 (26-Oct-2020)

- Fixed an issue where source information was sometimes not properly extracted from tests implemented as async methods.

## v1.1.1 (01-Oct-2020)

- Added a warning message when using this logger outside of GitHub Actions.

## v1.1 (27-Apr-2020)

- Added a switch to disable warnings which are reported if tests were ignored or skipped. Use `--logger "GitHubActions;report-warnings=false"` if you want to do that.
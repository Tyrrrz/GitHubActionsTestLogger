using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace GitHubActionsTestLogger;

internal static class TestSummary
{
    public static string Generate(
        string testSuiteName,
        string targetFrameworkName,
        TestRunStatistics testRunStatistics,
        IReadOnlyList<TestResult> testResults)
    {
        var buffer = new StringBuilder();

        // Spoiler header
        buffer
            .Append("<details>")
            .Append("<summary>")
            .Append(testRunStatistics.FailedTestCount <= 0 ? "🟢" : "🔴")
            .Append(" ")
            .Append("<b>")
            .Append(testSuiteName)
            .Append("</b>")
            .Append(" ")
            .Append("(")
            .Append(targetFrameworkName)
            .Append(")")
            .Append("</summary>")
            .Append("<br/>");

        // Overview table
        buffer
            // Table header
            .Append("<table>")
            .Append("<th width=\"99999\">")
            .Append("✓")
            .Append("&nbsp;")
            .Append("&nbsp;")
            .Append("Passed")
            .Append("</th>")
            .Append("<th width=\"99999\">")
            .Append("✘")
            .Append("&nbsp;")
            .Append("&nbsp;")
            .Append("Failed")
            .Append("</th>")
            .Append("<th width=\"99999\">")
            .Append("↷")
            .Append("&nbsp;")
            .Append("&nbsp;")
            .Append("Skipped")
            .Append("</th>")
            .Append("<th width=\"99999\">")
            .Append("∑")
            .Append("&nbsp;")
            .Append("&nbsp;")
            .Append("Total")
            .Append("</th>")
            .Append("<th width=\"99999\">")
            .Append("⧗")
            .Append("&nbsp;")
            .Append("&nbsp;")
            .Append("Elapsed")
            .Append("</th>")
            // Table body
            .Append("<tr>")
            .Append("<td align=\"center\">")
            .Append(
                testRunStatistics.PassedTestCount > 0
                    ? testRunStatistics.PassedTestCount.ToString()
                    : "—"
            )
            .Append("</td>")
            .Append("<td align=\"center\">")
            .Append(
                testRunStatistics.FailedTestCount > 0
                    ? testRunStatistics.FailedTestCount.ToString()
                    : "—"
            )
            .Append("</td>")
            .Append("<td align=\"center\">")
            .Append(
                testRunStatistics.SkippedTestCount > 0
                    ? testRunStatistics.SkippedTestCount.ToString()
                    : "—"
            )
            .Append("</td>")
            .Append("<td align=\"center\">")
            .Append(testRunStatistics.TotalTestCount)
            .Append("</td>")
            .Append("<td align=\"center\">")
            .Append(testRunStatistics.ElapsedDuration.ToHumanString())
            .Append("</td>")
            .Append("</tr>")
            .Append("</table>")
            .AppendLine()
            .AppendLine();

        // List of failed tests
        foreach (var testResult in testResults.Where(r => r.Outcome == TestOutcome.Failed))
        {
            // Generate permalink for the test
            var filePath = testResult.TryGetSourceFilePath();
            var fileLine = testResult.TryGetSourceLine();
            var url = !string.IsNullOrWhiteSpace(filePath)
                ? GitHubWorkflow.TryGenerateFilePermalink(filePath, fileLine)
                : null;

            buffer
                .Append("- Fail: ")
                .Append("[")
                .Append("**")
                .Append(testResult.TestCase.DisplayName)
                .Append("**")
                .Append("]")
                .Append("(")
                .Append(url ?? "#")
                .Append(")")
                .AppendLine()
                // YAML syntax highlighting works really well for exception messages and stack traces
                .AppendLine("```yml")
                .AppendLine(testResult.ErrorMessage)
                .AppendLine(testResult.ErrorStackTrace)
                .AppendLine("```");
        }

        // Spoiler closing tags
        buffer
            .Append("</details>")
            .AppendLine()
            .AppendLine();

        return buffer.ToString();
    }
}
using System;
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
        IReadOnlyList<TestResult> testResults)
    {
        var passedTestCount = testResults.Count(t => t.Outcome == TestOutcome.Passed);
        var failedTestCount = testResults.Count(t => t.Outcome == TestOutcome.Failed);
        var skippedTestCount = testResults.Count(t => t.Outcome == TestOutcome.Skipped);
        var totalTestCount = testResults.Count;
        var totalTestDuration = TimeSpan.FromMilliseconds(testResults.Sum(t => t.Duration.TotalMilliseconds));

        var buffer = new StringBuilder();

        // Spoiler header
        buffer
            .Append("<details>")
            .Append("<summary>")
            .Append(failedTestCount <= 0 ? "🟢" : "🔴")
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
                passedTestCount > 0
                    ? passedTestCount.ToString()
                    : "—"
            )
            .Append("</td>")
            .Append("<td align=\"center\">")
            .Append(
                failedTestCount > 0
                    ? failedTestCount.ToString()
                    : "—"
            )
            .Append("</td>")
            .Append("<td align=\"center\">")
            .Append(
                skippedTestCount > 0
                    ? skippedTestCount.ToString()
                    : "—"
            )
            .Append("</td>")
            .Append("<td align=\"center\">")
            .Append(totalTestCount)
            .Append("</td>")
            .Append("<td align=\"center\">")
            .Append(totalTestDuration.ToHumanString())
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
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
        var passedCount = testResults.Count(t => t.Outcome == TestOutcome.Passed);
        var failedCount = testResults.Count(t => t.Outcome == TestOutcome.Failed);
        var skippedCount = testResults.Count(t => t.Outcome == TestOutcome.Skipped);
        var totalCount = testResults.Count;
        var totalDuration = TimeSpan.FromMilliseconds(testResults.Sum(t => t.Duration.TotalMilliseconds));

        var buffer = new StringBuilder();

        // Header
        buffer
            .Append("<details>")
            .Append("<summary>")
            .Append(failedCount <= 0 ? "🟢" : "🔴")
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

        // Overview
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
                passedCount > 0
                    ? passedCount.ToString()
                    : "—"
            )
            .Append("</td>")
            .Append("<td align=\"center\">")
            .Append(
                failedCount > 0
                    ? failedCount.ToString()
                    : "—"
            )
            .Append("</td>")
            .Append("<td align=\"center\">")
            .Append(
                skippedCount > 0
                    ? skippedCount.ToString()
                    : "—"
            )
            .Append("</td>")
            .Append("<td align=\"center\">")
            .Append(totalCount)
            .Append("</td>")
            .Append("<td align=\"center\">")
            .Append(totalDuration.ToHumanString())
            .Append("</td>")
            .Append("</tr>")
            .Append("</table>")
            .AppendLine()
            .AppendLine();

        // Failed tests
        foreach (var testResult in testResults.Where(r => r.Outcome == TestOutcome.Failed))
        {
            // Generate permalink
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
                .AppendLine("```")
                .AppendLine(testResult.ErrorMessage)
                .AppendLine(testResult.ErrorStackTrace)
                .AppendLine("```");
        }

        buffer
            .Append("</details>")
            .AppendLine()
            .AppendLine();

        return buffer.ToString();
    }
}
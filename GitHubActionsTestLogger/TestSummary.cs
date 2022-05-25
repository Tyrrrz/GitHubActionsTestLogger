using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace GitHubActionsTestLogger;

internal static class TestSummary
{
    public static string Generate(
        TestRunCriteria testRunCriteria,
        ITestRunStatistics testRunStatistics,
        TimeSpan testRunElapsedTime,
        IReadOnlyList<TestResult> testResults)
    {
        var buffer = new StringBuilder();

        // Header
        buffer
            .Append("<details>")
            .Append("<summary>")
            .Append(testRunStatistics.GetFailedCount() <= 0 ? "🟢" : "🔴")
            .Append(" ")
            .Append("<b>")
            .AppendJoin(", ", testRunCriteria.Sources.Select(Path.GetFileNameWithoutExtension))
            .Append("</b>")
            .Append(" (")
            .Append(testRunCriteria.TryGetTargetFramework() ?? "Unknown Target Framework")
            .Append(")")
            .Append("</summary>")
            .Append("<br/>");

        // Overview
        buffer
            // Table header
            // Use symbols instead of emoji here to avoid visual collision with the header
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
                testRunStatistics.GetPassedCount() > 0
                    ? testRunStatistics.GetPassedCount().ToString()
                    : "—"
            )
            .Append("</td>")
            .Append("<td align=\"center\">")
            .Append(
                testRunStatistics.GetFailedCount() > 0
                    ? testRunStatistics.GetFailedCount().ToString()
                    : "—"
            )
            .Append("</td>")
            .Append("<td align=\"center\">")
            .Append(
                testRunStatistics.GetSkippedCount() > 0
                    ? testRunStatistics.GetSkippedCount().ToString()
                    : "—"
            )
            .Append("</td>")
            .Append("<td align=\"center\">")
            .Append(testRunStatistics.ExecutedTests)
            .Append("</td>")
            .Append("<td align=\"center\">")
            .Append(testRunElapsedTime.ToHumanString())
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
            var url = !string.IsNullOrWhiteSpace(filePath) && fileLine is not null
                ? GitHubWorkflow.TryGetFilePermalink(filePath, fileLine)
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
            .Append("<br/>")
            .Append("</details>")
            .AppendLine()
            .AppendLine();

        return buffer.ToString();
    }
}
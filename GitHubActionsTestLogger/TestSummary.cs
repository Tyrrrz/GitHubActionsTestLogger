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
            .Append(testRunStatistics.OverallOutcome switch
            {
                TestOutcome.Passed => "🟢",
                TestOutcome.Failed => "🔴",
                _ => "🟡"
            })
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

        // Test results
        var testResultsOrdered = testResults
            .OrderByDescending(r => r.Outcome == TestOutcome.Failed)
            .ThenByDescending(r => r.Outcome == TestOutcome.Passed);

        foreach (var testResult in testResultsOrdered)
        {
            // Generate permalink for the test source
            var filePath = testResult.TryGetSourceFilePath();
            var fileLine = testResult.TryGetSourceLine();
            var url = filePath?.Pipe(p => GitHubWorkflow.TryGenerateFilePermalink(p, fileLine));

            buffer
                .Append("- ")
                .Append(testResult.Outcome switch
                {
                    TestOutcome.Passed => "🟢",
                    TestOutcome.Failed => "🔴",
                    _ => "🟡"
                })
                .Append(" ");

            if (!string.IsNullOrWhiteSpace(url))
            {
                buffer
                    .Append("[");
            }

            buffer
                .Append("**")
                .Append(testResult.TestCase.DisplayName)
                .Append("**");

            if (!string.IsNullOrWhiteSpace(url))
            {
                buffer
                    .Append("]")
                    .Append("(")
                    .Append(url)
                    .Append(")");
            }

            buffer.AppendLine();

            if (!string.IsNullOrWhiteSpace(testResult.ErrorMessage))
            {
                // YAML syntax highlighting works really well for exception messages and stack traces
                buffer
                    .Append("   ").AppendLine("```yml")
                    .Append("   ").AppendLine(testResult.ErrorMessage)
                    .Append("   ").AppendLine(testResult.ErrorStackTrace)
                    .Append("   ").AppendLine("```");
            }
        }

        // Spoiler closing tag
        buffer
            .Append("</details>")
            .AppendLine()
            .AppendLine();

        return buffer.ToString();
    }
}
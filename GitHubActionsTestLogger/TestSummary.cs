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
        TestRunStatistics testRunStatistics,
        IReadOnlyList<TestResult> testResults)
    {
        var buffer = new StringBuilder();

        // Header
        buffer
            .Append("##")
            // Outcome
            .Append(" ")
            .Append(testRunStatistics.OverallOutcome switch
            {
                TestOutcome.Passed => "🟢",
                TestOutcome.Failed => "🔴",
                _ => "🟡"
            })
            // Suite name
            .Append(" ")
            .Append(testSuiteName)
            // Framework name & version
            .Append(" ")
            .Append("<sub>")
            .Append("<sup>")
            .Append("(")
            .Append(targetFrameworkName)
            .Append(")")
            .Append("</sub>")
            .Append("</sup>")
            .AppendLine();

        // Overview
        buffer
            // Table header
            .Append("<table>")
            // Passed
            .Append("<th width=\"99999\">")
            .Append("✓")
            .Append("&nbsp;")
            .Append("&nbsp;")
            .Append("Passed")
            .Append("</th>")
            // Failed
            .Append("<th width=\"99999\">")
            .Append("✘")
            .Append("&nbsp;")
            .Append("&nbsp;")
            .Append("Failed")
            .Append("</th>")
            // Skipped
            .Append("<th width=\"99999\">")
            .Append("↷")
            .Append("&nbsp;")
            .Append("&nbsp;")
            .Append("Skipped")
            .Append("</th>")
            // Total
            .Append("<th width=\"99999\">")
            .Append("∑")
            .Append("&nbsp;")
            .Append("&nbsp;")
            .Append("Total")
            .Append("</th>")
            // Elapsed
            .Append("<th width=\"99999\">")
            .Append("⧗")
            .Append("&nbsp;")
            .Append("&nbsp;")
            .Append("Elapsed")
            .Append("</th>")
            // Table body
            .Append("<tr>")
            // Passed
            .Append("<td align=\"center\">")
            .Append(
                testRunStatistics.PassedTestCount > 0
                    ? testRunStatistics.PassedTestCount.ToString()
                    : "—"
            )
            .Append("</td>")
            // Failed
            .Append("<td align=\"center\">")
            .Append(
                testRunStatistics.FailedTestCount > 0
                    ? testRunStatistics.FailedTestCount.ToString()
                    : "—"
            )
            .Append("</td>")
            // Skipped
            .Append("<td align=\"center\">")
            .Append(
                testRunStatistics.SkippedTestCount > 0
                    ? testRunStatistics.SkippedTestCount.ToString()
                    : "—"
            )
            .Append("</td>")
            // Total
            .Append("<td align=\"center\">")
            .Append(testRunStatistics.TotalTestCount)
            .Append("</td>")
            // Elapsed
            .Append("<td align=\"center\">")
            .Append(testRunStatistics.ElapsedDuration.ToHumanString())
            .Append("</td>")
            .Append("</tr>")
            .Append("</table>")
            .AppendLine()
            .AppendLine();

        // Test results
        var testResultGroups = testResults
            .GroupBy(r => r.TestCase.GetTypeFullyQualifiedName(), StringComparer.Ordinal)
            .Select(g => new
            {
                TypeFullyQualifiedName = g.Key,
                TypeName = g.First().TestCase.GetTypeMinimallyQualifiedName(),
                TestResults = g
                    .OrderByDescending(r => r.Outcome == TestOutcome.Failed)
                    .ThenByDescending(r => r.Outcome == TestOutcome.Passed)
                    .ThenBy(r => r.TestCase.DisplayName, StringComparer.Ordinal)
                    .ToArray()
            })
            .OrderByDescending(g => g.TestResults.Any(r => r.Outcome == TestOutcome.Failed))
            .ThenByDescending(g => g.TestResults.Any(r => r.Outcome == TestOutcome.Passed))
            .ThenBy(g => g.TypeName, StringComparer.Ordinal);

        foreach (var testResultGroup in testResultGroups)
        {
            // Test group spoiler
            buffer
                .Append("<details>")
                .Append("<summary>")
                // Group name
                .Append("<b>")
                .Append(testResultGroup.TypeName)
                .Append("</b>");

                // Failed test count
                if (testResultGroup.TestResults.Any(r => r.Outcome == TestOutcome.Failed))
                {
                    buffer
                        .Append(" ")
                        .Append("<i>")
                        .Append("(")
                        .Append(testResultGroup.TestResults.Count(r => r.Outcome == TestOutcome.Failed))
                        .Append(" ")
                        .Append("failed")
                        .Append(")")
                        .Append("</i>");
                }

                buffer
                .Append("</summary>")
                .Append("<p>")
                .AppendLine()
                .AppendLine();

            foreach (var testResult in testResultGroup.TestResults)
            {
                // Test source permalink
                var filePath = testResult.TryGetSourceFilePath();
                var fileLine = testResult.TryGetSourceLine();
                var url = filePath?.Pipe(p => GitHubWorkflow.TryGenerateFilePermalink(p, fileLine));

                buffer
                    .Append("  - ")
                    // Outcome
                    .Append(testResult.Outcome switch
                    {
                        TestOutcome.Passed => "🟩",
                        TestOutcome.Failed => "🟥",
                        _ => "🟨"
                    })
                    // Test name
                    .Append(" ");

                if (!string.IsNullOrWhiteSpace(url))
                {
                    buffer.Append("[");
                }

                buffer.Append(
                    // Use display name if it's different from the fully qualified name,
                    // otherwise use the minimally qualified name.
                    !string.Equals(
                        testResult.TestCase.DisplayName,
                        testResult.TestCase.FullyQualifiedName,
                        StringComparison.Ordinal
                    )
                        ? testResult.TestCase.DisplayName
                        : testResult.TestCase.GetMinimallyQualifiedName()
                );

                if (!string.IsNullOrWhiteSpace(url))
                {
                    buffer
                        .Append("]")
                        .Append("(")
                        .Append(url)
                        .Append(")");
                }

                buffer.AppendLine();

                // Error message & stack trace
                if (!string.IsNullOrWhiteSpace(testResult.ErrorMessage))
                {
                    // YAML syntax highlighting works really well for exception messages
                    buffer
                        .Append("    ").Append("```yml").AppendLine()
                        // Every line here should be indented, otherwise the formatting will break
                        .Append(testResult.ErrorMessage.Indent(4)).AppendLine()
                        .Append(testResult.ErrorStackTrace?.Indent(4)).AppendLine()
                        .Append("    ").Append("```").AppendLine();
                }
            }

            buffer
                // Close spoiler
                .Append("</p>")
                .Append("</details>")
                .AppendLine()
                .AppendLine();
        }

        // Footer
        buffer
            .Append("___")
            .AppendLine()
            .AppendLine();

        return buffer.ToString();
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace GitHubActionsTestLogger;

internal static class TestSummary
{
    public static string Generate(
        IReadOnlyList<string> testSources,
        IReadOnlyList<TestResult> testResults)
    {
        var title = testSources.Any()
            ? string.Join(", ", testSources.Select(s => Path.GetFileNameWithoutExtension(s)))
            : "Test report";

        var overallOutcome = !testResults.Any(x => x.Outcome == TestOutcome.Failed)
            ? TestOutcome.Passed
            : TestOutcome.Failed;

        var passedCount = testResults.Count(r => r.Outcome == TestOutcome.Passed);
        var skippedCount = testResults.Count(r => r.Outcome == TestOutcome.Skipped);
        var failedCount = testResults.Count(r => r.Outcome == TestOutcome.Failed);
        var totalCount = testResults.Count;
        var totalDuration = testResults.Sum(r => r.Duration.TotalSeconds).Pipe(TimeSpan.FromSeconds);

        var commonNamePrefix = testResults.Select(r => r.TestCase.FullyQualifiedName ?? "").GetLongestCommonPrefix();
        var testResultGroups = testResults
            .GroupBy(r => r
                .TestCase
                .FullyQualifiedName?
                .Substring(commonNamePrefix.Length)
                .SubstringUntilLast(".")
            );

        var buffer = new StringBuilder();

        buffer
            .Append("## 🧪 ")
            .AppendLine(title)
            .AppendLine();

        buffer
            .AppendLine("<table>")
            .AppendLine("<tr>")
            .AppendLine("<th width=\"99999\" align=\"center\">🟢 Passed</th>")
            .AppendLine("<th width=\"99999\" align=\"center\">🟡 Skipped</th>")
            .AppendLine("<th width=\"99999\" align=\"center\">🔴 Failed</th>")
            .AppendLine("<th width=\"99999\" align=\"center\">🔵 Total</th>")
            .AppendLine("<th width=\"99999\" align=\"center\">🕑 Elapsed</th>")
            .AppendLine("</tr>");

        buffer
            .AppendLine("<tr>")
            .Append("<td align=\"center\">")
            .Append(
                passedCount > 0
                    ? passedCount.ToString("N0", CultureInfo.InvariantCulture)
                    : "—"
            )
            .AppendLine("</td>")
            .Append("<td align=\"center\">")
            .Append(
                skippedCount > 0
                    ? skippedCount.ToString("N0", CultureInfo.InvariantCulture)
                    : "—"
            )
            .AppendLine("</td>")
            .Append("<td align=\"center\">")
            .Append(
                failedCount > 0
                    ? failedCount.ToString("N0", CultureInfo.InvariantCulture)
                    : "—"
            )
            .AppendLine("</td>")
            .Append("<td align=\"center\">")
            .Append(totalCount.ToString("N0", CultureInfo.InvariantCulture))
            .AppendLine("</td>")
            .Append("<td align=\"center\">")
            .Append(totalDuration.ToHumanReadableString())
            .AppendLine("</td>")
            .AppendLine("</table>")
            .AppendLine("<hr />");

        foreach (var testResultGroup in testResultGroups)
        {
            buffer
                .Append("### ")
                .AppendLine(testResultGroup.Key)
                .AppendLine();

            var groupTestResults = testResultGroup
                .OrderByDescending(r => r.Outcome)
                .ThenBy(r => r.TestCase.DisplayName)
                .ToArray();

            foreach (var testResult in groupTestResults)
            {
                buffer
                    .AppendLine("<details>")
                    .AppendLine("<summary>")
                    .Append(testResult.Outcome switch
                    {
                        TestOutcome.Passed => "🟢",
                        TestOutcome.Failed => "🔴",
                        _ => "🟡"
                    })
                    .Append(' ')
                    .Append(testResult.TestCase.DisplayName)
                    .AppendLine("</summary>")
                    .AppendLine()
                    .AppendLine("&nbsp;");

                buffer
                    .Append("- **Full name**: ")
                    .Append('`').Append(testResult.TestCase.FullyQualifiedName).AppendLine("`")
                    .Append("- **Outcome**: ")
                    .AppendLine(testResult.Outcome.ToString())
                    .Append("- **Elapsed**: ")
                    .AppendLine(testResult.Duration.ToHumanReadableString());

                if (!string.IsNullOrWhiteSpace(testResult.ErrorMessage))
                {
                    buffer
                        .AppendLine("- **Error**:")
                        .AppendLine()
                        .AppendLine("```")
                        .AppendLine(testResult.ErrorMessage)
                        .AppendLine(testResult.ErrorStackTrace)
                        .AppendLine("```");
                }

                buffer
                    .AppendLine()
                    .AppendLine("</details>");
            }
        }

        buffer
            .AppendLine("<hr />")
            .AppendLine("<sub>")
            .Append("<a href=\"https://github.com/Tyrrrz/GitHubActionsTestLogger\">")
            .Append("Generated by .NET GitHub Actions Test Logger")
            .AppendLine("</a>")
            .AppendLine("</sub>");

        return buffer.ToString();
    }
}
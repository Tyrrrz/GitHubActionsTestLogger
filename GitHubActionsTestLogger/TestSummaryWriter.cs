using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace GitHubActionsTestLogger;

internal class TestSummaryWriter : IDisposable
{
    private readonly FileStream _stream;
    private readonly StreamWriter _writer;
    private readonly long _offset;

    private bool IsInitial => _offset == 0;

    public TestSummaryWriter(string filePath)
    {
        _stream = File.OpenWrite(filePath);
        _writer = new StreamWriter(_stream, Encoding.UTF8)
        {
            // Shorter newline to save space
            NewLine = "\n"
        };

        // Summary file might already contain data from a previous test run.
        // We need to remember where the existing data ends to not accidentally
        // overwrite it as we will be moving the position of the stream backwards.
        _offset = Math.Max(0, _stream.Length - 1);
    }

    // This method will be called multiple times per test run because there is no hook
    // we can rely on to know when all tests in a run have finished executing.
    // For this reason, we will be continuously overwriting a portion of the file with new data.
    public void Update(TestRunCriteria testRunCriteria, IReadOnlyList<TestResult> testResults)
    {
        var passedCount = testResults.Count(r => r.Outcome == TestOutcome.Passed);
        var failedCount = testResults.Count(r => r.Outcome == TestOutcome.Failed);
        var skippedCount = testResults.Count(r => r.Outcome == TestOutcome.Skipped);
        var totalCount = testResults.Count;
        var totalDuration = testResults.Sum(r => r.Duration.TotalSeconds).Pipe(TimeSpan.FromSeconds);

        // Seek to where the previous summary ends (if it exists)
        _stream.Seek(_offset, SeekOrigin.Begin);

        // Header or margin
        if (IsInitial)
        {
            _writer.WriteLine("### Test Results");
            _writer.WriteLine();
        }

        _writer.Write("<table>");

        // Header
        _writer.Write("<tr>");
        _writer.Write("<th/>");
        _writer.Write("<th width=\"9999\">Suite</th>");
        _writer.Write("<th>Passed</th>");
        _writer.Write("<th>Failed</th>");
        _writer.Write("<th>Skipped</th>");
        _writer.Write("<th>Total</th>");
        _writer.Write("<th>Elapsed</th>");
        _writer.Write("</tr>");

        // Overview
        _writer.Write("<tr>");
        _writer.Write("<td>");
        _writer.Write(failedCount <= 0 ? "✔" : "❌");
        _writer.Write("</td>");
        _writer.Write("<td>");
        _writer.Write("<b>");
        _writer.Write(string.Join(", ", testRunCriteria.Sources.Select(Path.GetFileNameWithoutExtension)));
        _writer.Write("</b>");
        _writer.Write($" ({testRunCriteria.TryGetTargetFramework()})");
        _writer.Write("</td>");
        _writer.Write("<td>");
        _writer.Write(passedCount > 0
            ? passedCount.ToString("N0", CultureInfo.InvariantCulture)
            : "—"
        );
        _writer.Write("</td>");
        _writer.Write("<td>");
        _writer.Write(failedCount > 0
            ? failedCount.ToString("N0", CultureInfo.InvariantCulture)
            : "—"
        );
        _writer.Write("</td>");
        _writer.Write("<td>");
        _writer.Write(skippedCount > 0
            ? skippedCount.ToString("N0", CultureInfo.InvariantCulture)
            : "—"
        );
        _writer.Write("</td>");
        _writer.Write("<td>");
        _writer.Write(totalCount.ToString("N0", CultureInfo.InvariantCulture));
        _writer.Write("</td>");
        _writer.Write("<td>");
        _writer.Write(totalDuration.ToHumanString());
        _writer.Write("</td>");
        _writer.Write("</tr>");
        _writer.WriteLine("</table>");

        // Failed tests
        if (failedCount > 0)
        {
            _writer.Write("<details>");
            _writer.Write("<summary>");
            _writer.Write("<h4>Failed tests</h4>");
            _writer.Write("</summary>");
            _writer.WriteLine();
            _writer.WriteLine();

            foreach (var testResult in testResults.Where(r => r.Outcome == TestOutcome.Failed))
            {
                _writer.Write("##### ");
                _writer.WriteLine(testResult.TestCase.DisplayName);
                _writer.WriteLine();

                _writer.WriteLine("```");
                _writer.WriteLine(testResult.ErrorMessage);
                _writer.WriteLine(testResult.ErrorStackTrace);
                _writer.WriteLine("```");
            }

            _writer.WriteLine("</details>");
        }

        _writer.WriteLine("___");
        _writer.WriteLine();
        _writer.Flush();
    }

    public void Dispose()
    {
        _stream.Dispose();
        _writer.Dispose();
    }
}
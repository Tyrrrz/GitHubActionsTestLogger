using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GitHubActionsTestLogger
{
    // Adapted from https://github.com/atifaziz/StackTraceParser

    internal class StackTraceParser
    {
        private const string Space = @"[\x20\t]";
        private const string NotSpace = @"[^\x20\t]";

        private static readonly Regex Pattern = new Regex(@"
            ^
            " + Space + @"*
            \w+ " + Space + @"+
            (?<frame>
                (?<type> " + NotSpace + @"+ ) \.
                (?<method> " + NotSpace + @"+? ) " + Space + @"*
                (?<params>  \( ( " + Space + @"* \)
                               |                    (?<pt> .+?) " + Space + @"+ (?<pn> .+?)
                                 (, " + Space + @"* (?<pt> .+?) " + Space + @"+ (?<pn> .+?) )* \) ) )
                ( " + Space + @"+
                    ( # Microsoft .NET stack traces
                    \w+ " + Space + @"+
                    (?<file> ( [a-z] \: # Windows rooted path starting with a drive letter
                             | / )      # *nix rooted path starting with a forward-slash
                             .+? )
                    \: \w+ " + Space + @"+
                    (?<line> [0-9]+ ) \p{P}?
                    | # Mono stack traces
                    \[0x[0-9a-f]+\] " + Space + @"+ \w+ " + Space + @"+
                    <(?<file> [^>]+ )>
                    :(?<line> [0-9]+ )
                    )
                )?
            )
            \s*
            $",
            RegexOptions.IgnoreCase
            | RegexOptions.Multiline
            | RegexOptions.ExplicitCapture
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled,
            // Cap the evaluation time to make it obvious should the expression
            // fall into the "catastrophic backtracking" trap due to over
            // generalization.
            // https://github.com/atifaziz/StackTraceParser/issues/4
            TimeSpan.FromSeconds(5));

        private static int? ParseNullableInt(string? text) =>
            int.TryParse(text, out var value) ? value : default;

        public static IEnumerable<StackFrame> Parse(string text) =>
            from Match m in Pattern.Matches(text)
            select m.Groups into groups
            select new StackFrame(
                groups["method"].Value,
                groups["file"].Value,
                ParseNullableInt(groups["line"].Value));
    }

    internal class StackFrame
    {
        public string Call { get; }

        public string? FilePath { get; }

        public int? Line { get; }

        public StackFrame(string call, string? filePath, int? line)
        {
            Call = call;
            FilePath = filePath;
            Line = line;
        }
    }
}
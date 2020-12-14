using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GitHubActionsTestLogger.Internal.Extensions;

namespace GitHubActionsTestLogger.Internal
{
    // Adapted from https://github.com/atifaziz/StackTraceParser

    internal partial class StackFrame
    {
        // We're only interested in file path and line number

        public string MethodCall { get; }

        public string? FilePath { get; }

        public int? Line { get; }

        public StackFrame(string methodCall, string? filePath, int? line)
        {
            MethodCall = methodCall;
            FilePath = filePath;
            Line = line;
        }
    }

    internal partial class StackFrame
    {
        private const string Space = @"[\x20\t]";
        private const string NotSpace = @"[^\x20\t]";

        private static readonly Regex Pattern = new(@"
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
            RegexOptions.IgnoreCase |
            RegexOptions.Multiline |
            RegexOptions.ExplicitCapture |
            RegexOptions.CultureInvariant |
            RegexOptions.IgnorePatternWhitespace |
            RegexOptions.Compiled,
            // Cap the evaluation time to make it obvious should the expression
            // fall into the "catastrophic backtracking" trap due to over
            // generalization.
            // https://github.com/atifaziz/StackTraceParser/issues/4
            TimeSpan.FromSeconds(5));

        public static IEnumerable<StackFrame> ParseMany(string text) =>
            from Match m in Pattern.Matches(text)
            select m.Groups
            into groups
            select new StackFrame(
                groups["type"].Value + '.' + groups["method"].Value,
                groups["file"].Value,
                groups["line"].Value.ParseNullableIntOrDefault()
            );
    }
}
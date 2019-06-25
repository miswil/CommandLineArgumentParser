using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CommandLineArgumentParser
{
    public static class CommandLineSplitter
    {
        /// <summary>
        /// split the argument under the rule at https://docs.microsoft.com/en-us/cpp/c-language/parsing-c-command-line-arguments?view=vs-2019 or https://docs.microsoft.com/en-us/cpp/cpp/parsing-cpp-command-line-arguments?view=vs-2019
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static IEnumerable<string> Split(string argument)
        {
            var pattern = new Regex(@"\G[ \t]*(?<arg>(?:[^ \t\\""]|(?<!\\)(?:\\\\)*""(?:[^\\""]|(?:\\\\)*\\""|\\+(?!""))*?(?:(?:\\\\)*""|$)|(?<!\\)(?:\\\\)*\\""|(?<!\\)\\+(?!""))+)");
            return pattern.Matches(argument)
                .Cast<Match>()
                .Select(m => m.Groups["arg"].Value)
                .Select(s => s
                    .ReplaceRegex(@"(?<!\\)(?:\\\\)*""",
                        match => match.Value.ReplaceRegex(@"""", string.Empty).ReplaceRegex(@"\\\\", @"\"))
                    .ReplaceRegex(@"(?<!\\)(?:\\\\)*\\""",
                        match => match.Value.ReplaceRegex(@"\\""", @"""").ReplaceRegex(@"\\\\", @"\")));
        }
    }
    static class RegexUtility
    {
        public static string ReplaceRegex(this string input, string pattern, string replacement)
            => Regex.Replace(input, pattern, replacement);
        public static string ReplaceRegex(this string input, string pattern, MatchEvaluator evaluator)
            => Regex.Replace(input, pattern, evaluator);
    }
}
using CommandLineArgumentParser;
using System.Collections.Generic;
using Xunit;

namespace XUnitTestProject1
{
    public class UnitTest1
    {
        [Theory]
        [MemberData(nameof(MyData))]
        [MemberData(nameof(MsData))]
        public void Test1(string input, string[] expected)
        {
            var actual = ArgumentSplitter.Split(input);
            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> MyData = new[]
        {
            new  object[]{ "a", new string[] { "a" } },
            new  object[]{ " a", new string[] { "a" } },
            new  object[]{ "a ", new string[] { "a" } },
            new  object[]{ @"a\", new string[] { @"a\" } },
            new  object[]{ @"a\""", new string[] { @"a""" } },
            new  object[]{ @"a\"" b", new string[] { @"a""", "b" } },
            new  object[]{ @"a \""b", new string[] { @"a", @"""b" } },
        };

        public static IEnumerable<object[]> MsData = new[]
        {
            new  object[]{ @"""a b c"" d e", new string[] { "a b c", "d", "e" } },
            new  object[]{ @"""abc"" d e", new string[] { "abc", "d", "e" } },
            new  object[]{ @"a\\b d""e f""g h", new string[] { @"a\\b", "de fg", "h" } },
            new  object[]{ @"a\\\""b c d", new string[] { @"a\""b", "c", "d" } },
            new  object[]{ @"a\\\\""b c"" d e", new string[] { @"a\\b c", "d", "e" } },
            new  object[]{ @"""ab\""c"" ""\\"" d", new string[] { @"ab""c", @"\", "d" } },
            new  object[]{ @"a\\\b d""e f""g h", new string[] { @"a\\\b", @"de fg", "h" } },
        };
    }
}
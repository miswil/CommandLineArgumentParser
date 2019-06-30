using CommandLineArgumentParser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Xunit;

namespace CommandLineArgumentParser.Test
{
    public class ArgvParserTest
    {
        [Theory]
        [InlineData("-")]
        [InlineData("/")]
        [InlineData("/", "-")]
        [InlineData("-", "/")]
        public void TestIsShortOption(params string[] shortNamedOptionPrefix)
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                MultipleShortNamedOptionWithOneTokenEnabled = false,
                NonSeparatedShortNamedOptionArgumentEnabled = false,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = shortNamedOptionPrefix,
            };

            var input = parser.Parse<Input>(new[] { $"{shortNamedOptionPrefix[0]}b" });
            Assert.True(input.BooleanOption);
        }

        [Theory]
        [InlineData("-")]
        [InlineData("/")]
        [InlineData("/", "-")]
        [InlineData("-", "/")]
        [InlineData(null)]
        public void TestIsShortOptionNot(params string[] shortNamedOptionPrefix)
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                MultipleShortNamedOptionWithOneTokenEnabled = false,
                NonSeparatedShortNamedOptionArgumentEnabled = false,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = shortNamedOptionPrefix,
            };

            var input = parser.Parse<Input>(new[] { "true" });
            Assert.False(input.BooleanOption);
        }

        [Fact]
        public void TestIsShortOptionNotNoBody()
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                MultipleShortNamedOptionWithOneTokenEnabled = false,
                NonSeparatedShortNamedOptionArgumentEnabled = false,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
            };

            var input = parser.Parse<Input>(new[] { "-" });
            Assert.False(input.BooleanOption);
        }

        [Fact]
        public void TestParseBooleanShortOption()
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                MultipleShortNamedOptionWithOneTokenEnabled = false,
                NonSeparatedShortNamedOptionArgumentEnabled = false,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
            };

            var input = parser.Parse<Input>(new[] { "-b" });
            Assert.True(input.BooleanOption);
        }

        [Fact]
        public void TestParseBooleanShortOptionMulti()
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                MultipleShortNamedOptionWithOneTokenEnabled = true,
                NonSeparatedShortNamedOptionArgumentEnabled = false,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
            };

            var input = parser.Parse<Input>(new[] { "-bv" });
            Assert.True(input.BooleanOption);
            Assert.True(input.BooleanOption2);
        }

        [Fact]
        public void TestParseBooleanShortOptionInvalid()
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                MultipleShortNamedOptionWithOneTokenEnabled = false,
                NonSeparatedShortNamedOptionArgumentEnabled = false,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
            };

            Assert.Throws<InvalidArgumentFormatException>(() => parser.Parse<Input>(new[] { "-bv" }));
            parser.MultipleShortNamedOptionWithOneTokenEnabled = true;
            Assert.Throws<UnknownOptionSpecifiedException>(() => parser.Parse<Input>(new[] { "-bq" }));
            Assert.Throws<OptionConvertException>(() => parser.Parse<Input>(new[] { "-bi" }));
        }

        [Fact]
        public void TestParseValueShortOption()
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                MultipleShortNamedOptionWithOneTokenEnabled = false,
                NonSeparatedShortNamedOptionArgumentEnabled = false,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
            };

            var input = parser.Parse<Input>(new[] { "-s", "str", "-i", "10", "-d", "10.0", "-e", "One" });
            Assert.Equal("str", input.StringOption);
            Assert.Equal(10, input.IntegerOption);
            Assert.Equal(10.0, input.DoubleOption);
            Assert.Equal(Enumerable.One, input.EnumerableOption);
        }

        [Fact]
        public void TestParseValueShortOptionNonSeparate()
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                MultipleShortNamedOptionWithOneTokenEnabled = false,
                NonSeparatedShortNamedOptionArgumentEnabled = true,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
            };

            var input = parser.Parse<Input>(new[] { "-sstr", "-i10", "-d10.0", "-eOne" });
            Assert.Equal("str", input.StringOption);
            Assert.Equal(10, input.IntegerOption);
            Assert.Equal(10.0, input.DoubleOption);
            Assert.Equal(Enumerable.One, input.EnumerableOption);
        }

        [Fact]
        public void TestParseValueShortOptionNonSeparateInvalid()
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                MultipleShortNamedOptionWithOneTokenEnabled = false,
                NonSeparatedShortNamedOptionArgumentEnabled = false,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
            };

            Assert.Throws<InvalidArgumentFormatException>(() => parser.Parse<Input>(new[] { "-sstr", "-i10", "-d10.0", "-eOne" }));
        }

        [Fact]
        public void TestParseValueShortOptionConvert()
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                MultipleShortNamedOptionWithOneTokenEnabled = false,
                NonSeparatedShortNamedOptionArgumentEnabled = false,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
            };

            var input = parser.Parse<Input>(new[] { "-t", "str", "-j", "10", "-f", "10.0", "-g", "One" });
            Assert.Equal("hoge", input.StringConvertOption);
            Assert.Equal(100, input.IntegerConvertOption);
            Assert.Equal(100.0, input.DoubleConvertOption);
            Assert.Equal(Enumerable.Two, input.EnumerableConvertOption);
        }

        [Fact]
        public void TestParseValueShortOptionInvalid()
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                MultipleShortNamedOptionWithOneTokenEnabled = false,
                NonSeparatedShortNamedOptionArgumentEnabled = false,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
            };

            Assert.Throws<UnknownOptionSpecifiedException>(() => parser.Parse<Input>(new[] { "-z" }));
            Assert.Throws<OptionConvertException>(() => parser.Parse<Input>(new[] { "-i", "10.0" }));
            Assert.Throws<OptionConvertException>(() => parser.Parse<Input>(new[] { "-d", "hoge" }));
            Assert.Throws<OptionConvertException>(() => parser.Parse<Input>(new[] { "-e", "Hoge" }));
            Assert.Throws<InvalidArgumentFormatException>(() => parser.Parse<Input>(new[] { "-i" }));
            Assert.Throws<InvalidArgumentFormatException>(() => parser.Parse<Input>(new[] { "-d" }));
            Assert.Throws<InvalidArgumentFormatException>(() => parser.Parse<Input>(new[] { "-e" }));
        }

        [Theory]
        [InlineData("--")]
        [InlineData("/")]
        [InlineData("/", "--")]
        [InlineData("--", "/")]
        public void TestIsLongOption(params string[] longNamedOptionPrefix)
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                LongNamedOptionArgumentAssignCharacter = null,
                LongNamedOptionEnabled = true,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = longNamedOptionPrefix,
            };

            var input = parser.Parse<Input>(new[] { $"{longNamedOptionPrefix[0]}boolean" });
            Assert.True(input.BooleanOption);
        }

        [Theory]
        [InlineData("--")]
        [InlineData("/")]
        [InlineData("/", "--")]
        [InlineData("--", "/")]
        [InlineData(null)]
        public void TestIsLongOptionNot(params string[] longNamedOptionPrefix)
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                LongNamedOptionArgumentAssignCharacter = null,
                LongNamedOptionEnabled = true,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = longNamedOptionPrefix,
            };

            var input = parser.Parse<Input>(new[] { "-v" });
            Assert.False(input.BooleanOption);
        }

        [Fact]
        public void TestIsLongOptionNotNoBody()
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                LongNamedOptionArgumentAssignCharacter = null,
                LongNamedOptionEnabled = true,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
            };

            Assert.Throws<UnknownOptionSpecifiedException>(() => parser.Parse<Input>(new[] { "--" }));
        }

        [Fact]
        public void TestParseBooleanLongOption()
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                LongNamedOptionArgumentAssignCharacter = null,
                LongNamedOptionEnabled = true,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
            };

            var input = parser.Parse<Input>(new[] { "--boolean", "--boolean2" });
            Assert.True(input.BooleanOption);
            Assert.True(input.BooleanOption2);
        }

        [Fact]
        public void TestParseValueLongOption()
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                LongNamedOptionArgumentAssignCharacter = null,
                LongNamedOptionEnabled = true,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
            };

            var input = parser.Parse<Input>(new[] { "--string", "str", "--integer", "10", "--double", "10.0", "--enum", "One" });
            Assert.Equal("str", input.StringOption);
            Assert.Equal(10, input.IntegerOption);
            Assert.Equal(10.0, input.DoubleOption);
            Assert.Equal(Enumerable.One, input.EnumerableOption);
        }

        [Fact]
        public void TestParseValueLongOptionEqual()
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                LongNamedOptionArgumentAssignCharacter = new[] { '=' },
                LongNamedOptionEnabled = true,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
            };

            var input = parser.Parse<Input>(new[] { "--string=str", "--integer=10", "--double=10.0", "--enum=One" });
            Assert.Equal("str", input.StringOption);
            Assert.Equal(10, input.IntegerOption);
            Assert.Equal(10.0, input.DoubleOption);
            Assert.Equal(Enumerable.One, input.EnumerableOption);
            input = parser.Parse<Input>(new[] { "--string", "str", "--integer", "10", "--double", "10.0", "--enum", "One" });
            Assert.Equal("str", input.StringOption);
            Assert.Equal(10, input.IntegerOption);
            Assert.Equal(10.0, input.DoubleOption);
            Assert.Equal(Enumerable.One, input.EnumerableOption);
        }

        [Fact]
        public void TestParseValueLongOptionColon()
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                LongNamedOptionArgumentAssignCharacter = new[] { ':' },
                LongNamedOptionEnabled = true,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
            };

            var input = parser.Parse<Input>(new[] { "--string:str", "--integer:10", "--double:10.0", "--enum:One" });
            Assert.Equal("str", input.StringOption);
            Assert.Equal(10, input.IntegerOption);
            Assert.Equal(10.0, input.DoubleOption);
            Assert.Equal(Enumerable.One, input.EnumerableOption);
            input = parser.Parse<Input>(new[] { "--string", "str", "--integer", "10", "--double", "10.0", "--enum", "One" });
            Assert.Equal("str", input.StringOption);
            Assert.Equal(10, input.IntegerOption);
            Assert.Equal(10.0, input.DoubleOption);
            Assert.Equal(Enumerable.One, input.EnumerableOption);
        }        

        [Fact]
        public void TestParseValueLongOptionConvert()
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                LongNamedOptionArgumentAssignCharacter = null,
                LongNamedOptionEnabled = true,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
            };

            var input = parser.Parse<Input>(new[] { "--string2", "str", "--integer2", "10", "--double2", "10.0", "--enum2", "One" });
            Assert.Equal("hoge", input.StringConvertOption);
            Assert.Equal(100, input.IntegerConvertOption);
            Assert.Equal(100.0, input.DoubleConvertOption);
            Assert.Equal(Enumerable.Two, input.EnumerableConvertOption);
        }

        [Fact]
        public void TestParseValueLongOptionInvalid()
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                LongNamedOptionArgumentAssignCharacter = null,
                LongNamedOptionEnabled = true,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
            };

            Assert.Throws<UnknownOptionSpecifiedException>(() => parser.Parse<Input>(new[] { "--hoge" }));
            Assert.Throws<OptionConvertException>(() => parser.Parse<Input>(new[] { "--integer", "10.0" }));
            Assert.Throws<OptionConvertException>(() => parser.Parse<Input>(new[] { "--double", "hoge" }));
            Assert.Throws<OptionConvertException>(() => parser.Parse<Input>(new[] { "--enum", "Hoge" }));
            Assert.Throws<InvalidArgumentFormatException>(() => parser.Parse<Input>(new[] { "--integer" }));
            Assert.Throws<InvalidArgumentFormatException>(() => parser.Parse<Input>(new[] { "--double" }));
            Assert.Throws<InvalidArgumentFormatException>(() => parser.Parse<Input>(new[] { "--enum" }));
        }

        [Fact]
        public void TestNoOption()
        {
            var parser = new ArgvParser
            {
                IntermixedOerandEnabled = true,
                LongNamedOptionArgumentAssignCharacter = null,
                LongNamedOptionEnabled = true,
                OperandDelimiter = new[] { "--" },
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
            };
            var input = parser.Parse<Input>(new string[] { "string" });
            Assert.False(input.BooleanOption);
            Assert.False(input.BooleanOperand);
            Assert.Equal("string", input.StringOperand);
            Assert.Equal(default, input.IntegerOption);
            Assert.Equal(default, input.IntegerOperand);
            Assert.Equal(default, input.DoubleOption);
            Assert.Equal(default, input.DoubleOperand);
            Assert.Equal(default, input.EnumerableOption);
            Assert.Equal(default, input.EnumerableOperand);
        }

        [Fact]
        public void TestInvalidOption()
        {
            var parser = new ArgvParser
            {
                IntermixedOerandEnabled = true,
                LongNamedOptionArgumentAssignCharacter = null,
                LongNamedOptionEnabled = true,
                OperandDelimiter = new[] { "--" },
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
            };
            Assert.Throws<ArgumentException>(() => parser.Parse<Input>(new string[] { null }));
            Assert.Throws<ArgumentException>(() => parser.Parse<Input>(new string[] { "-i", null }));
        }

        [Fact]
        public void TestParseSubCommand()
        {
            var parser = new ArgvParser
            {
                IntermixedOerandEnabled = true,
                LongNamedOptionArgumentAssignCharacter = null,
                LongNamedOptionEnabled = true,
                OperandDelimiter = new[] { "--" },
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
                SubCommandEnabled = true,
            };
            var input = parser.Parse<Input4>(new[] { "-s", "str", "-i", "10", "-d", "10.0", "-e", "One", "sub", "-s", "str2", "-i", "11", "-d", "11.0", "-e", "Two" });
            Assert.Equal("str", input.StringOption);
            Assert.Equal(10, input.IntegerOption);
            Assert.Equal(10.0, input.DoubleOption);
            Assert.Equal(Enumerable.One, input.EnumerableOption);
            Assert.Equal("str2", input.SubCommand.StringOption);
            Assert.Equal(11, input.SubCommand.IntegerOption);
            Assert.Equal(11.0, input.SubCommand.DoubleOption);
            Assert.Equal(Enumerable.Two, input.SubCommand.EnumerableOption);
        }

        [Fact]
        public void TestParseSubCommandAfterOperand()
        {
            var parser = new ArgvParser
            {
                IntermixedOerandEnabled = true,
                LongNamedOptionArgumentAssignCharacter = null,
                LongNamedOptionEnabled = true,
                OperandDelimiter = new[] { "--" },
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
                SubCommandEnabled = true,
            };
            var input = parser.Parse<Input4>(new[] { "-s", "str", "-i", "10", "-d", "10.0", "-e", "One", "str2", "true", "11", "11.0", "Two", "Rest", "of", "main", "operand", "sub", "-s", "str3", "-i", "12", "-d", "12.0", "-e", "Three", "str4", "true", "13", "13.0", "Four", "Rest", "of", "Subcommand", "operand"});
            Assert.Equal("str", input.StringOption);
            Assert.Equal(10, input.IntegerOption);
            Assert.Equal(10.0, input.DoubleOption);
            Assert.Equal(Enumerable.One, input.EnumerableOption);
            Assert.Equal("str2", input.StringOperand);
            Assert.True(input.BooleanOperand);
            Assert.Equal(11, input.IntegerOperand);
            Assert.Equal(11.0, input.DoubleOperand);
            Assert.Equal(Enumerable.Two, input.EnumerableOperand);
            Assert.Equal(new[] { "Rest", "of", "main", "operand" }, input.RestAll);
            Assert.Equal("str3", input.SubCommand.StringOption);
            Assert.Equal(12, input.SubCommand.IntegerOption);
            Assert.Equal(12.0, input.SubCommand.DoubleOption);
            Assert.Equal(Enumerable.Three, input.SubCommand.EnumerableOption);
            Assert.Equal("str4", input.SubCommand.StringOperand);
            Assert.True(input.SubCommand.BooleanOperand);
            Assert.Equal(13, input.SubCommand.IntegerOperand);
            Assert.Equal(13.0, input.SubCommand.DoubleOperand);
            Assert.Equal(Enumerable.Four, input.SubCommand.EnumerableOperand);
            Assert.Equal(new[] { "Rest", "of", "Subcommand", "operand" }, input.SubCommand.RestAll);
        }

        [Fact]
        public void TestParseSubCommandAfterOperandDelimeter()
        {
            var parser = new ArgvParser
            {
                IntermixedOerandEnabled = true,
                LongNamedOptionArgumentAssignCharacter = null,
                LongNamedOptionEnabled = true,
                OperandDelimiter = new[] { "--" },
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
                SubCommandEnabled = true,
            };
            var input = parser.Parse<Input4>(new[] { "-s", "str", "--", "-i","true", "10", "10.0", "One", "Rest", "of", "main", "operand", "sub", "-s", "str2", "--", "-i", "true", "12", "12.0", "Three", "Rest", "of", "Subcommand", "operand" });
            Assert.Equal("str", input.StringOption);
            Assert.Equal(0, input.IntegerOption);
            Assert.Equal(0.0, input.DoubleOption);
            Assert.Equal(Enumerable.Zero, input.EnumerableOption);
            Assert.Equal("-i", input.StringOperand);
            Assert.True(input.BooleanOperand);
            Assert.Equal(10, input.IntegerOperand);
            Assert.Equal(10.0, input.DoubleOperand);
            Assert.Equal(Enumerable.One, input.EnumerableOperand);
            Assert.Equal(new[] { "Rest", "of", "main", "operand" }, input.RestAll);
            Assert.Equal("str2", input.SubCommand.StringOption);
            Assert.Equal(0, input.SubCommand.IntegerOption);
            Assert.Equal(0.0, input.SubCommand.DoubleOption);
            Assert.Equal(Enumerable.Zero, input.SubCommand.EnumerableOption);
            Assert.Equal("-i", input.SubCommand.StringOperand);
            Assert.True(input.SubCommand.BooleanOperand);
            Assert.Equal(12, input.SubCommand.IntegerOperand);
            Assert.Equal(12.0, input.SubCommand.DoubleOperand);
            Assert.Equal(Enumerable.Three, input.SubCommand.EnumerableOperand);
            Assert.Equal(new[] { "Rest", "of", "Subcommand", "operand" }, input.SubCommand.RestAll);
        }

        [Fact]
        public void TestParseSubCommandVirtual()
        {
            var parser = new ArgvParser
            {
                IntermixedOerandEnabled = true,
                LongNamedOptionArgumentAssignCharacter = null,
                LongNamedOptionEnabled = true,
                OperandDelimiter = new[] { "--" },
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
                SubCommandEnabled = true,
            };
            var input = parser.Parse<Input5>(new[] { "-s", "str", "-i", "10", "-d", "10.0", "-e", "One", "Rest", "of", "main", "operand", "sub1", "-i", "10" });
            Assert.Equal("str", input.StringOption);
            Assert.Equal(10, input.IntegerOption);
            Assert.Equal(10.0, input.DoubleOption);
            Assert.Equal(Enumerable.One, input.EnumerableOption);
            Assert.Equal(new[] { "Rest", "of", "main", "operand" }, input.RestAll);
            Assert.Equal(10, input.SubCommand.Do());
            input = parser.Parse<Input5>(new[] { "-s", "str", "-i", "10", "-d", "10.0", "-e", "One", "Rest", "of", "main", "operand", "sub2", "-d", "10.0" });
            Assert.Equal("str", input.StringOption);
            Assert.Equal(10, input.IntegerOption);
            Assert.Equal(10.0, input.DoubleOption);
            Assert.Equal(Enumerable.One, input.EnumerableOption);
            Assert.Equal(new[] { "Rest", "of", "main", "operand" }, input.RestAll);
            Assert.Equal(10.0, input.SubCommand.Do());
        }

        [Fact]
        public void TestParseOperand()
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                LongNamedOptionArgumentAssignCharacter = null,
                LongNamedOptionEnabled = true,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
            };

            var input = parser.Parse<Input>(new[] { "string", "true", "10", "10.0", "One", "rest", "of", "the", "arguments" });
            Assert.Equal("string", input.StringOperand);
            Assert.True(input.BooleanOperand);
            Assert.Equal(10, input.IntegerOperand);
            Assert.Equal(10.0, input.DoubleOperand);
            Assert.Equal(Enumerable.One, input.EnumerableOperand);
            Assert.Equal(new[] { "rest", "of", "the", "arguments" }, input.RestAll);
        }

        [Fact]
        public void TestParseOperandInvalidTypeRestAll()
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                LongNamedOptionArgumentAssignCharacter = null,
                LongNamedOptionEnabled = true,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
            };

            Assert.Throws<OperandConvertException>(() => parser.Parse<Input3>(new[] { "string", "true", "10", "10.0", "One", "rest", "of", "the", "arguments" }));
        }

        [Fact]
        public void TestParseOperandConvert()
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                LongNamedOptionArgumentAssignCharacter = null,
                LongNamedOptionEnabled = true,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
            };

            var input = parser.Parse<Input2>(new[] { "string", "10", "10.0", "One" });
            Assert.Equal("hoge", input.StringConvertOperand);
            Assert.Equal(100, input.IntegerConvertOperand);
            Assert.Equal(100.0, input.DoubleConvertOperand);
        }

        [Fact]
        public void TestParseOperandInvalid()
        {
            ArgvParser parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                LongNamedOptionArgumentAssignCharacter = null,
                LongNamedOptionEnabled = true,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
            };

            Assert.Throws<OperandConvertException>(() => parser.Parse<Input>(new[] { "string", "int", "10.0" }));
            Assert.Throws<OperandConvertException>(() => parser.Parse<Input>(new[] { "string", "10", "double" }));
            Assert.Throws<OperandConvertException>(() => parser.Parse<Input>(new[] { "string", "10", "10.0", "Three" }));
            Assert.Throws<OperandConvertException>(() => parser.Parse<Input>(new[] { "string", "10", "10.0", "Three", "rest", "of", "the", "operand" }));
            Assert.Throws<TooManyArgumentOperandException>(() => parser.Parse<Input2>(new[] { "str", "10", "10.0", "One", "Waste" }));
        }

        [Fact]
        public void TestOptionOperandMixed()
        {
            var parser = new ArgvParser
            {
                IntermixedOerandEnabled = false,
                LongNamedOptionArgumentAssignCharacter = null,
                LongNamedOptionEnabled = true,
                OperandDelimiter = null,
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
            };
            Assert.ThrowsAny<Exception>(() => parser.Parse<Input>(new[] { "-b", "str", "-i", "10", "true", "-d", "10.0", "11", "11.0", "-e", "One", "Two" }));
            parser.IntermixedOerandEnabled = true;
            var input = parser.Parse<Input>(new[] { "-b", "str", "-i", "10", "true", "-d", "10.0", "11", "11.0", "-e", "One", "Two" });
            Assert.True(input.BooleanOption);
            Assert.Equal("str", input.StringOperand);
            Assert.Equal(10, input.IntegerOption);
            Assert.True(input.BooleanOperand);
            Assert.Equal(10.0, input.DoubleOption);
            Assert.Equal(11.0, input.DoubleOperand);
            Assert.Equal(Enumerable.One, input.EnumerableOption);
            Assert.Equal(Enumerable.Two, input.EnumerableOperand);
        }

        [Fact]
        public void TestOperandDelimiter()
        {
            var parser = new ArgvParser
            {
                IntermixedOerandEnabled = true,
                LongNamedOptionArgumentAssignCharacter = null,
                LongNamedOptionEnabled = true,
                OperandDelimiter = new[] { "--" },
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
            };

            var input = parser.Parse<Input>(new[] { "--", "-b", "true", "11", "11.0", "Two", "-i", "10" });
            Assert.False(input.BooleanOption);
            Assert.Equal("-b", input.StringOperand);
            Assert.Equal(0, input.IntegerOption);
            Assert.True(input.BooleanOperand);
            Assert.Equal(11, input.IntegerOperand);
            Assert.Equal(0.0, input.DoubleOption);
            Assert.Equal(11.0, input.DoubleOperand);
            Assert.Equal(Enumerable.Zero, input.EnumerableOption);
            Assert.Equal(Enumerable.Two, input.EnumerableOperand);
            Assert.Equal(new[] { "-i", "10" }, input.RestAll);
        }
    }


    public enum Enumerable
    {
        Zero, One, Two, Three, Four
    }

    public class Input
    {
        [Option('s', "string")]
        public string StringOption { get; set; }
        [Option('b', "boolean")]
        public bool BooleanOption { get; set; }
        [Option('v', "boolean2")]
        public bool BooleanOption2 { get; set; }
        [Option('i', "integer")]
        public int IntegerOption { get; set; }
        [Option('d', "double")]
        public double DoubleOption { get; set; }
        [Option('e', "enum")]
        public Enumerable EnumerableOption { get; set; }

        [Option('t', "string2", ConverterType = typeof(StringConverter))]
        public string StringConvertOption { get; set; }
        [Option('j', "integer2", ConverterType = typeof(IntegerConverter))]
        public int IntegerConvertOption { get; set; }
        [Option('f', "double2", ConverterType = typeof(DoubleConverter))]
        public double DoubleConvertOption { get; set; }
        [Option('g', "enum2", ConverterType = typeof(EnumConverter))]
        public Enumerable EnumerableConvertOption { get; set; }

        [Operand(0)]
        public string StringOperand { get; set; }
        [Operand(1)]
        public bool BooleanOperand { get; set; }
        [Operand(2)]
        public int IntegerOperand { get; set; }
        [Operand(3)]
        public double DoubleOperand { get; set; }
        [Operand(4)]
        public Enumerable EnumerableOperand { get; set; }
        [Operand]
        public List<string> RestAll { get; set; }
    }
    public class Input2
    {
        [Operand(0, ConverterType = typeof(StringConverter))]
        public string StringConvertOperand { get; set; }
        [Operand(1, ConverterType = typeof(IntegerConverter))]
        public int IntegerConvertOperand { get; set; }
        [Operand(2, ConverterType = typeof(DoubleConverter))]
        public double DoubleConvertOperand { get; set; }
        [Operand(3, ConverterType = typeof(EnumConverter))]
        public Enumerable EnumerableConvertOperand { get; set; }
    }

    public class Input3
    {
        [Option('s', "string")]
        public string StringOption { get; set; }
        [Option('b', "boolean")]
        public bool BooleanOption { get; set; }
        [Option('v', "boolean2")]
        public bool BooleanOption2 { get; set; }
        [Option('i', "integer")]
        public int IntegerOption { get; set; }
        [Option('d', "double")]
        public double DoubleOption { get; set; }
        [Option('e', "enum")]
        public Enumerable EnumerableOption { get; set; }

        [Option('t', "string2", ConverterType = typeof(StringConverter))]
        public string StringConvertOption { get; set; }
        [Option('j', "integer2", ConverterType = typeof(IntegerConverter))]
        public int IntegerConvertOption { get; set; }
        [Option('f', "double2", ConverterType = typeof(DoubleConverter))]
        public double DoubleConvertOption { get; set; }
        [Option('g', "enum2", ConverterType = typeof(EnumConverter))]
        public Enumerable EnumerableConvertOption { get; set; }

        [Operand(0)]
        public string StringOperand { get; set; }
        [Operand(1)]
        public bool BooleanOperand { get; set; }
        [Operand(2)]
        public int IntegerOperand { get; set; }
        [Operand(3)]
        public double DoubleOperand { get; set; }
        [Operand(4)]
        public Enumerable EnumerableOperand { get; set; }
        [Operand]
        public string RestAll { get; set; }
    }

    public class Input5
    {
        [Option('s', "string")]
        public string StringOption { get; set; }
        [Option('b', "boolean")]
        public bool BooleanOption { get; set; }
        [Option('v', "boolean2")]
        public bool BooleanOption2 { get; set; }
        [Option('i', "integer")]
        public int IntegerOption { get; set; }
        [Option('d', "double")]
        public double DoubleOption { get; set; }
        [Option('e', "enum")]
        public Enumerable EnumerableOption { get; set; }
        [Operand]
        public List<string> RestAll { get; set; }

        [SubCommand("sub1", typeof(SubCommand1))]
        [SubCommand("sub2", typeof(SubCommand2))]
        public ISubCommand SubCommand { get; set; }
    }

    public interface ISubCommand
    {
        object Do();
    }

    public class SubCommand1 : ISubCommand
    {
        [Option('i')]
        public int IntegerProperty { get; set; }
        public object Do()
        {
            return IntegerProperty;
        }
    }

    public class SubCommand2 : ISubCommand
    {
        [Option('d')]
        public double DoubleProperty { get; set; }
        public object Do()
        {
            return DoubleProperty;
        }
    }

    public class Input4 : Input
    {
        [SubCommand("sub")]
        public Input SubCommand { get; set; }
    }

    public class StringConverter : TypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return "hoge";
        }
    }

    public class IntegerConverter : TypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return 100;
        }
    }

    public class DoubleConverter : TypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return 100.0;
        }
    }

    public class EnumConverter : TypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return Enumerable.Two;
        }
    }
}
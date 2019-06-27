using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace CommandLineArgumentParser
{
    public class ArgvParser
    {
        #region Static
        private static ArgvParser _POSIX;
        public static ArgvParser POSIX =>
            _POSIX ?? (_POSIX = new ArgvParser
            {
                LongNamedOptionEnabled = false,
                NonSeparatedShortNamedOptionArgumentEnabled = true,
                IntermixedOerandEnabled = false,
                MultipleShortNamedOptionWithOneTokenEnabled = true,
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
                OperandDelimiter = null,
                LongNamedOptionArgumentAssignCharacter = null,
            });

        private static ArgvParser _GNU;
        public static ArgvParser GNU =>
            _GNU ?? (_GNU = new ArgvParser
            {
                LongNamedOptionEnabled = true,
                NonSeparatedShortNamedOptionArgumentEnabled = true,
                IntermixedOerandEnabled = true,
                MultipleShortNamedOptionWithOneTokenEnabled = true,
                ShortNamedOptionPrefix = new[] { "-" },
                LongNamedOptionPrefix = new[] { "--" },
                OperandDelimiter = new[] { "--" },
                LongNamedOptionArgumentAssignCharacter = new[] { '=' },
            });

        private static ArgvParser _windows;
        public static ArgvParser Windows =>
            _windows ?? (_windows = new ArgvParser
            {
                LongNamedOptionEnabled = true,
                NonSeparatedShortNamedOptionArgumentEnabled = false,
                IntermixedOerandEnabled = true,
                MultipleShortNamedOptionWithOneTokenEnabled = false,
                ShortNamedOptionPrefix = new[] { "/" },
                LongNamedOptionPrefix = new[] { "/" },
                LongNamedOptionArgumentAssignCharacter = null,
                OperandDelimiter = null,
            });
        #endregion Static
        
        #region Option
        /// <summary>
        /// true: allow use of the long named option. e.g. '--help'
        /// </summary>
        public bool LongNamedOptionEnabled { get; set; }

        /// <summary>
        /// true: allow the option argument not to be seprated by whitespace.
        /// e.g. '-ofilename' is treated as if '-o' is an option and 'filename' is option argument of '-o' option.
        /// </summary>
        public bool NonSeparatedShortNamedOptionArgumentEnabled { get; set; }

        /// <summary>
        /// false: the operands must be passed exactly after all the options.
        /// true: the operands and the options may be passed as any order.
        /// </summary>
        public bool IntermixedOerandEnabled { get; set; }

        /// <summary>
        /// true: multiple options can be passed in a single token if the option do not take the argument.
        /// </summary>
        public bool MultipleShortNamedOptionWithOneTokenEnabled { get; set; }

        /// <summary>
        /// The argument is interpreted as short named option after this prefix.
        /// </summary>
        public IEnumerable<string> ShortNamedOptionPrefix { get; set; }

        /// <summary>
        /// The argument is interpreted as long named option after this prefix.
        /// </summary>
        public IEnumerable<string> LongNamedOptionPrefix { get; set; }

        /// <summary>
        /// true: The argument which terminates all options; any following arguments are treated as operands, even if they begin with a hyphen.
        /// e.g. '-a -- -b' is treated as if '-a' as an option and '-b' as an operand.
        /// </summary>
        public IEnumerable<string> OperandDelimiter { get; set; }

        /// <summary>
        /// allow the option argument parameter to be passed with a form separated by the characters. e.g. --output=filename.
        /// </summary>
        public IEnumerable<char> LongNamedOptionArgumentAssignCharacter { get; set; }
        #endregion Option

        #region Method
        public void Parse(object stored, IEnumerable<string> argv)
        {
            var parseInformation = new ParseInformation();
            parseInformation.Stored = stored ?? throw new ArgumentNullException(nameof(stored));
            var argl = argv?.ToList() ?? throw new ArgumentNullException(nameof(argv));
            parseInformation.OptionProperties = stored.GetType()
                .GetProperties()
                .Where(p =>
                p.GetCustomAttributes()
                .Any(a => a.GetType() == typeof(OptionAttribute)))
                .Select(p => new OptionProperty { Attribute = (OptionAttribute)p.GetCustomAttributes().First(a => a is OptionAttribute), Property = p })
                .ToList();
            parseInformation.OperandProperties = stored.GetType()
                .GetProperties()
                .Where(p =>
                    p.GetCustomAttributes()
                    .Any(a => a.GetType() == typeof(OperandAttribute)))
                .Select(p => new OperandProperty { Attribute = (OperandAttribute)p.GetCustomAttributes().First(a => a is OperandAttribute), Property = p })
                .ToList();
            parseInformation.RestAll = new List<string>();
            // TODO implement fluent api

            int operandIndex = 0;
            bool isRestOperand = false;
            for (int index = 0; index < argl.Count;)
            {
                if (this.OperandDelimiter?.Contains(argl[index]) ?? false)
                {
                    isRestOperand = true;
                    ++index;
                    continue;
                }
                int readCount = 0;
                if (!isRestOperand)
                {
                    readCount = this.ParseOption(parseInformation, argl.Skip(index));
                    index += readCount;
                }
                if (readCount == 0)
                {
                    isRestOperand = isRestOperand || !this.IntermixedOerandEnabled;
                    do
                    {
                        this.ParseOperand(parseInformation, argl[index], operandIndex);
                        ++operandIndex;
                        ++index;
                    } while (isRestOperand && index < argl.Count);
                }
            }
        }

        public TStore Parse<TStore>(IEnumerable<string> argv) where TStore : new()
        {
            TStore stored = new TStore();
            this.Parse(stored, argv);
            return stored;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argv"></param>
        /// <returns>the number of parsed arguments</returns>
        private int ParseOption(ParseInformation parseInformation, IEnumerable<string> argv)
        {
            if (!argv.Any())
            {
                return 0;
            }
            string fst = argv.First();
            if (fst == null)
            {
                throw new ArgumentException("an element of aragv must not be null.");
            }
            string scd = argv.Skip(1).Any()
                ? (argv.Skip(1).First()
                    ?? throw new ArgumentException("an element of aragv must not be null."))
                : null;

            return this.ParseOption(parseInformation, fst, scd);
        }

        private int ParseOption(ParseInformation parseInformation, string fst, string scd)
        {
            var isShortOption = this.IsShoftOption(fst, out string shortOptionBody);
            var isLongOption = this.IsLongOption(fst, out string longOptionBody) && this.LongNamedOptionEnabled;

            if (isShortOption)
            {
                try
                {
                    return this.ParseShortOption(parseInformation, shortOptionBody, scd);
                }
                catch
                {
                    if (!isLongOption)
                    {
                        throw;
                    }
                }
            }
            if (isLongOption)
            {
                return this.ParseLongOption(parseInformation, longOptionBody, scd);
            }
            return 0;
        }

        private bool IsShoftOption(string arg, out string argBody)
        {
            if (this.ShortNamedOptionPrefix == null)
            {
                argBody = null;
                return false;
            }

            argBody = this.ShortNamedOptionPrefix
                .Where(prefix => arg.StartsWith(prefix))
                .Select(prefix => arg.Remove(0, prefix.Length))
                .FirstOrDefault();
            return !string.IsNullOrEmpty(argBody);
        }

        private int ParseShortOption(ParseInformation parseInformation, string argBody, string scd)
        {
            var optionProperty = parseInformation.OptionProperties
                .FirstOrDefault(o => o.Attribute.ShortPrefix == argBody[0]);
            if (optionProperty != null)
            {
                Type optionArgType = optionProperty.Property.PropertyType;
                if (optionArgType == typeof(bool))
                {
                    this.ParseBooleanShortOption(parseInformation, argBody);
                    return 1;
                }
                else
                {
                    return this.ParseValueShortOption(parseInformation, optionProperty, argBody, scd);
                }
            }
            else
            {
                throw new UnknownOptionSpecifiedException("An unknown option is specified")
                {
                    Option = argBody
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argBody"></param>
        /// <exception cref="InvalidArgumentFormatException">Two or more options are specified when only one option is allowed.</exception>
        /// <exception cref="UnknownOptionSpecifiedException">Unknown option is specified.</exception>
        /// <exception cref="OptionConvertException">An option which is not boolean is specified.</exception>
        /// <exception cref="AggregateException">All exceptions thrown when full parsed.</exception>
        private void ParseBooleanShortOption(ParseInformation parseInformation, string argBody)
        {
            if (argBody.Length > 1 && !this.MultipleShortNamedOptionWithOneTokenEnabled)
            {
                throw new InvalidArgumentFormatException("Too many characters are specified as option")
                {
                    Option = argBody,
                };
            }

            var targetProperties = argBody
                .Select(
                    argChar =>
                    new
                    {
                        ArgChar = argChar,
                        OptionProperty = parseInformation.OptionProperties.FirstOrDefault(o => o.Attribute.ShortPrefix == argChar)
                    });

            foreach (var checkedOption in targetProperties)
            {
                if (checkedOption.OptionProperty == null)
                {
                    throw new UnknownOptionSpecifiedException($"An unknown option is specified.")
                    {
                        Option = checkedOption.ArgChar.ToString(),
                    };
                }
                if (checkedOption.OptionProperty?.Property.PropertyType != typeof(bool))
                {
                    throw new OptionConvertException($"Failed to convert the option to boolean.")
                    {
                        Option = checkedOption.ArgChar.ToString(),
                    };
                }
            }

            foreach (var otherOption in targetProperties)
            {
                otherOption.OptionProperty.Property.SetValue(parseInformation.Stored, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidArgumentFormatException">An option argument is specified not separated with option.</exception>
        /// <exception cref="OptionConvertException">An option argument cannot convert to appropriate type.</exception>
        private int ParseValueShortOption(ParseInformation parseInformation, OptionProperty optionProperty, string argBody, string scd)
        {
            if (!this.NonSeparatedShortNamedOptionArgumentEnabled && argBody.Length > 1)
            {
                throw new InvalidArgumentFormatException("Option argument must be separated.")
                {
                    Option = argBody,
                };
            }

            string optionArg;
            int ret;
            if (this.NonSeparatedShortNamedOptionArgumentEnabled && argBody.Length > 1)
            {
                optionArg = argBody.Substring(1);
                ret = 1;
            }
            else
            {
                if (!string.IsNullOrEmpty(scd))
                {
                    optionArg = scd;
                    ret = 2;
                }
                else
                {
                    throw new InvalidArgumentFormatException("An option argument must be specified.")
                    {
                        Option = argBody
                    };
                }
            }
            TypeConverter converter;
            if (optionProperty.Attribute.ConverterType == null)
            {
                converter = TypeDescriptor.GetConverter(optionProperty.Property.PropertyType);
            }
            else
            {
                converter = (TypeConverter)Activator.CreateInstance(optionProperty.Attribute.ConverterType);
            }

            try
            {
                optionProperty.Property.SetValue(parseInformation.Stored, converter.ConvertFrom(optionArg));
            }
            catch (Exception nsex)
            {
                throw new OptionConvertException("Failed to convert option argument", nsex)
                {
                    Option = argBody[0].ToString(),
                    Argument = optionArg,
                };
            }
            return ret;
        }

        private bool IsLongOption(string arg, out string argBody)
        {
            if (this.LongNamedOptionPrefix == null)
            {
                argBody = null;
                return false;
            }
            
            argBody = this.LongNamedOptionPrefix
                .Where(prefix => arg.StartsWith(prefix))
                .Select(prefix => arg.Remove(0, prefix.Length))
                .FirstOrDefault();
            return !string.IsNullOrEmpty(argBody);
        }

        private int ParseLongOption(ParseInformation parseInformation, string argBody, string scd)
        {
            var optionProperty = parseInformation.OptionProperties
                .FirstOrDefault(o =>
                {
                    return
                     o.Attribute.LongPrefix == argBody
                     ||
                     (this.LongNamedOptionArgumentAssignCharacter != null &&
                      this.LongNamedOptionArgumentAssignCharacter.Any(c => argBody.StartsWith($"{o.Attribute.LongPrefix}{c}")));
                });
            if (optionProperty != null)
            {
                Type optionArgType = optionProperty.Property.PropertyType;
                if (optionArgType == typeof(bool) && optionProperty.Attribute.LongPrefix == argBody)
                {
                    optionProperty.Property.SetValue(parseInformation.Stored, true);
                    return 1;
                }
                else
                {
                    return this.ParseValueLongOption(parseInformation, optionProperty, argBody, scd);
                }
            }
            else
            {
                throw new UnknownOptionSpecifiedException("An unknown option is specified")
                {
                    Option = argBody
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidArgumentFormatException">An option argument is specified not separated with option.</exception>
        /// <exception cref="OptionConvertException">An option argument cannot convert to appropriate type.</exception>
        private int ParseValueLongOption(ParseInformation parseInformation, OptionProperty optionProperty, string argBody, string scd)
        {
            string optionArg = null;
            int ret = 0;
            if (this.LongNamedOptionArgumentAssignCharacter != null)
            {
                int index = this.LongNamedOptionArgumentAssignCharacter.Select(c => argBody.IndexOf(c)).Where(i => i != -1).FirstOrDefault();
                if (index > 0)
                {
                    optionArg = argBody.Substring(index + 1);
                    ret = 1;
                }
            }
            if (optionArg == null)
            {
                if (!string.IsNullOrEmpty(scd))
                {
                    optionArg = scd;
                    ret = 2;
                }
                else
                {
                    throw new InvalidArgumentFormatException("An option argument must be specified.")
                    {
                        Option = argBody
                    };
                }
            }

            TypeConverter converter;
            if (optionProperty.Attribute.ConverterType == null)
            {
                converter = TypeDescriptor.GetConverter(optionProperty.Property.PropertyType);
            }
            else
            {
                converter = (TypeConverter)Activator.CreateInstance(optionProperty.Attribute.ConverterType);
            }
            try
            {
                optionProperty.Property.SetValue(parseInformation.Stored, converter.ConvertFrom(optionArg));
            }
            catch (Exception nsex)
            {
                throw new OptionConvertException("Failed to convert an option argument", nsex)
                {
                    Option = argBody[0].ToString(),
                    Argument = optionArg,
                };
            }
            return ret;
        }

        private void ParseOperand(ParseInformation parseInformation, string arg, int operandIndex)
        {
            var operandProperty = parseInformation.OperandProperties
                .FirstOrDefault(o => o.Attribute.Index == operandIndex);

            if (operandProperty != null)
            {
                this.ParseOperand(parseInformation, arg, operandProperty);
            }
            else
            {
                operandProperty = parseInformation.OperandProperties
                                .FirstOrDefault(o => o.Attribute.Index == OperandAttribute.RestAll);

                if (operandProperty != null)
                {
                    this.ParseRestOperand(parseInformation, arg, operandProperty);
                }
                else
                {
                    throw new TooManyArgumentOperandException("Too many operands are specified.")
                    {
                        Operand = arg
                    };
                }
            }
        }

        private void ParseOperand(ParseInformation parseInformation, string arg, OperandProperty operandProperty)
        {
            TypeConverter converter;
            if (operandProperty.Attribute.ConverterType == null)
            {
                converter = TypeDescriptor.GetConverter(operandProperty.Property.PropertyType);
            }
            else
            {
                converter = (TypeConverter)Activator.CreateInstance(operandProperty.Attribute.ConverterType);
            }
            try
            {
                operandProperty.Property.SetValue(parseInformation.Stored, converter.ConvertFrom(arg));
            }
            catch (Exception nsex)
            {
                throw new OperandConvertException("Failed to convert option argument", nsex)
                {
                    Operand = arg
                };
            }
        }

        private void ParseRestOperand(ParseInformation parseInformation, string arg, OperandProperty operandProperty)
        {
            if (operandProperty.Property.PropertyType.IsAssignableFrom(parseInformation.RestAll.GetType()))
            {
                operandProperty.Property.SetValue(parseInformation.Stored, parseInformation.RestAll);
                parseInformation.RestAll.Add(arg);
            }
            else
            {
                throw new OperandConvertException("input type for the rest all operand must be ICollection<string>")
                {
                    Operand = arg,
                };
            }
        }

        private class OptionProperty
        {
            public OptionAttribute Attribute { get; set; }
            public PropertyInfo Property { get; set; }
        }

        private class OperandProperty
        {
            public OperandAttribute Attribute { get; set; }
            public PropertyInfo Property { get; set; }
        }
        #endregion

        #region class
        private class ParseInformation
        {
            public List<OptionProperty> OptionProperties { get; set; }
            public List<OperandProperty> OperandProperties { get; set; }
            public List<string> RestAll { get; set; }
            public object Stored { get; set; }
        }
        #endregion
    }
}

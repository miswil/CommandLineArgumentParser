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
                SubCommandEnabled = false,
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
                SubCommandEnabled = false,
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
                SubCommandEnabled = false,
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
        /// true: sub command and trailing arguments for sub command is parsed.
        /// </summary>
        public bool SubCommandEnabled { get; set; }

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
            var argl = argv?.Select(arg => arg ?? throw new ArgumentException("an element of aragv must not be null.")).ToList() ?? throw new ArgumentNullException(nameof(argv));
            parseInformation.OptionProperties = this.GetProperties<OptionAttribute>(stored);
            parseInformation.OperandProperties = this.GetProperties<OperandAttribute>(stored);
            parseInformation.SubCommandProperties = this.GetProperties<SubCommandAttribute>(stored);
            parseInformation.RestAll = new List<string>();
            // TODO implement fluent api

            int operandIndex = 0;
            bool isRestOperand = false;
            for (int index = 0; index < argl.Count;)
            {
                if (!isRestOperand && (this.OperandDelimiter?.Contains(argl[index]) ?? false))
                {
                    isRestOperand = true;
                    ++index;
                }
                else if (!isRestOperand && this.TryParseOption(parseInformation, argl[index], index + 1 < argl.Count ? argl[index + 1] : null, out int readCount))
                {
                    index += readCount;
                }
                else if (this.SubCommandEnabled && this.TryParseSubCommand(parseInformation, argl[index], argl.Skip(index + 1)))
                {
                    break;
                }
                else
                {
                    isRestOperand = isRestOperand || !this.IntermixedOerandEnabled;
                    this.ParseOperand(parseInformation, argl[index], operandIndex);
                    ++operandIndex;
                    ++index;
                }
            }
        }

        public TStore Parse<TStore>(IEnumerable<string> argv) where TStore : new()
        {
            TStore stored = new TStore();
            this.Parse(stored, argv);
            return stored;
        }

        private List<StoredProperty<TAttribute>> GetProperties<TAttribute>(object stored)
            where TAttribute : Attribute
        {
            return stored.GetType()
                .GetProperties()
                .SelectMany(p => p.GetCustomAttributes(typeof(TAttribute)).Select(a => new StoredProperty<TAttribute> { Attribute = (TAttribute)a, Property = p }))
                .ToList();
        }

        private bool TryParseOption(ParseInformation parseInformation, string fst, string scd, out int readCount)
        {
            var isShortOption = this.IsShoftOptionFormat(fst, out string shortOptionBody);
            var isLongOption = this.IsLongOptionFormat(fst, out string longOptionBody) && this.LongNamedOptionEnabled;

            if (isShortOption)
            {
                try
                {
                    return this.TryParseShortOption(parseInformation, shortOptionBody, scd, out readCount);
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
                return this.TryParseLongOption(parseInformation, longOptionBody, scd, out readCount);
            }
            readCount = 0;
            return false;
        }

        private bool IsShoftOptionFormat(string arg, out string argBody)
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

        private bool TryParseShortOption(ParseInformation parseInformation, string argBody, string scd, out int readCount)
        {
            var optionProperty = parseInformation.OptionProperties
                .FirstOrDefault(o => o.Attribute.ShortPrefix == argBody[0]);
            if (optionProperty != null)
            {
                Type optionArgType = optionProperty.Property.PropertyType;
                if (optionArgType == typeof(bool))
                {
                    this.ParseBooleanShortOption(parseInformation, argBody);
                    readCount = 1;
                    return true;
                }
                else
                {
                    this.TryParseValueShortOption(parseInformation, optionProperty, argBody, scd, out readCount);
                    return true;
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
                if (checkedOption.OptionProperty.Property.PropertyType != typeof(bool))
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
        private void TryParseValueShortOption(ParseInformation parseInformation, StoredProperty<OptionAttribute> optionProperty, string argBody, string scd, out int readCount)
        {
            if (!this.NonSeparatedShortNamedOptionArgumentEnabled && argBody.Length > 1)
            {
                throw new InvalidArgumentFormatException("Option argument must be separated.")
                {
                    Option = argBody,
                };
            }

            string optionArg;
            if (this.NonSeparatedShortNamedOptionArgumentEnabled && argBody.Length > 1)
            {
                optionArg = argBody.Substring(1);
                readCount = 1;
            }
            else
            {
                if (!string.IsNullOrEmpty(scd))
                {
                    optionArg = scd;
                    readCount = 2;
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
        }

        private bool IsLongOptionFormat(string arg, out string argBody)
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

        private bool TryParseLongOption(ParseInformation parseInformation, string argBody, string scd, out int readCount)
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
                    readCount = 1;
                    return true;
                }
                else
                {
                    this.ParseValueLongOption(parseInformation, optionProperty, argBody, scd, out readCount);
                    return true;
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
        private void ParseValueLongOption(ParseInformation parseInformation, StoredProperty<OptionAttribute> optionProperty, string argBody, string scd, out int readCount)
        {
            string optionArg = null;
            readCount = 0;
            if (this.LongNamedOptionArgumentAssignCharacter != null)
            {
                int index = this.LongNamedOptionArgumentAssignCharacter.Select(c => argBody.IndexOf(c)).Where(i => i != -1).FirstOrDefault();
                if (index > 0)
                {
                    optionArg = argBody.Substring(index + 1);
                    readCount = 1;
                }
            }
            if (optionArg == null)
            {
                if (!string.IsNullOrEmpty(scd))
                {
                    optionArg = scd;
                    readCount = 2;
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
        }

        private bool TryParseSubCommand(ParseInformation parseInformation, string subCommand, IEnumerable<string> argv)
        {
            if (this.IsSubCommand(subCommand, parseInformation, out var subCommandProperty))
            {
                var subCommandArgStore = Activator.CreateInstance(subCommandProperty.Attribute.SubCommandType ?? subCommandProperty.Property.PropertyType);
                this.Parse(subCommandArgStore, argv);
                subCommandProperty.Property.SetValue(parseInformation.Stored, subCommandArgStore);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsSubCommand(string arg, ParseInformation parseInformation, out StoredProperty<SubCommandAttribute> subCommandProperty)
        {
            subCommandProperty = parseInformation.SubCommandProperties.FirstOrDefault(p => p.Attribute.SubCommand == arg);
            return subCommandProperty != null;
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

        private void ParseOperand(ParseInformation parseInformation, string arg, StoredProperty<OperandAttribute> operandProperty)
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

        private void ParseRestOperand(ParseInformation parseInformation, string arg, StoredProperty<OperandAttribute> operandProperty)
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
        #endregion

        #region class
        private class StoredProperty<TAttribute>
            where TAttribute : Attribute
        {
            public TAttribute Attribute { get; set; }
            public PropertyInfo Property { get; set; }
        }

        private class ParseInformation
        {
            public List<StoredProperty<OptionAttribute>> OptionProperties { get; set; }
            public List<StoredProperty<OperandAttribute>> OperandProperties { get; set; }
            public List<StoredProperty<SubCommandAttribute>> SubCommandProperties { get; set; }

            public List<string> RestAll { get; set; }
            public object Stored { get; set; }
        }
        #endregion
    }
}

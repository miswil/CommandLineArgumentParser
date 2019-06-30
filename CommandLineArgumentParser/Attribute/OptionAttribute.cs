using System;

namespace CommandLineArgumentParser
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionAttribute : ArgumentAttribute
    {
        public char ShortPrefix { get; set; }
        public string LongPrefix { get; set; }
        public OptionAttribute(char shortPrefix, string longPrefix = null)
        {
            this.ShortPrefix = shortPrefix;
            this.LongPrefix = longPrefix;
        }
    }
}

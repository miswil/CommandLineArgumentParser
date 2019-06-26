using System;

namespace CommandLineArgumentParser
{
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

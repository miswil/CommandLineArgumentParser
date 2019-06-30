using System;
using System.Collections.Generic;
using System.Text;

namespace CommandLineArgumentParser
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class SubCommandAttribute : Attribute
    {
        public string SubCommand { get; set; }

        public Type SubCommandType { get; set; }

        public SubCommandAttribute(string subCommand, Type subCommandType = null)
        {
            this.SubCommand = subCommand;
            this.SubCommandType = subCommandType;
        }
    }
}

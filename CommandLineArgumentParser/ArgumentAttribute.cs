using System;
using System.Collections.Generic;
using System.Text;

namespace CommandLineArgumentParser
{
    public class ArgumentAttribute : Attribute
    {
        public Type ConverterType { get; set; }

        protected ArgumentAttribute()
        {

        }
    }
}

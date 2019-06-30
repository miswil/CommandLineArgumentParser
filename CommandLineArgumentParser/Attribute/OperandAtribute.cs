using System;

namespace CommandLineArgumentParser
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OperandAttribute : ArgumentAttribute
    {
        public const int RestAll = -1;

        public int Index { get; set; }

        public OperandAttribute(int index = RestAll)
        {
            this.Index = index;
        }
    }
}

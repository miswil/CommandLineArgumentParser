using System;
using System.Collections.Generic;
using System.Text;

namespace CommandLineArgumentParser
{

    [Serializable]
    public class ParseException : Exception
    {
        public ParseException() { }
        public ParseException(string message) : base(message) { }
        public ParseException(string message, Exception inner) : base(message, inner) { }
        protected ParseException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class OptionParseException : ParseException
    {
        public string Option { get; set; }
        public OptionParseException() { }
        public OptionParseException(string message) : base(message) { }
        public OptionParseException(string message, Exception inner) : base(message, inner) { }
        protected OptionParseException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class UnknownOptionSpecifiedException : OptionParseException
    {
        public UnknownOptionSpecifiedException() { }
        public UnknownOptionSpecifiedException(string message) : base(message) { }
        public UnknownOptionSpecifiedException(string message, Exception inner) : base(message, inner) { }
        protected UnknownOptionSpecifiedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class InvalidArgumentFormatException : OptionParseException
    {
        public InvalidArgumentFormatException() { }
        public InvalidArgumentFormatException(string message) : base(message) { }
        public InvalidArgumentFormatException(string message, Exception inner) : base(message, inner) { }
        protected InvalidArgumentFormatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class OptionConvertException : OptionParseException
    {
        public string Argument { get; set; }
        public OptionConvertException() { }
        public OptionConvertException(string message) : base(message) { }
        public OptionConvertException(string message, Exception inner) : base(message, inner) { }
        protected OptionConvertException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class OperandParseException : ParseException
    {
        public string Operand { get; set; }
        public OperandParseException() { }
        public OperandParseException(string message) : base(message) { }
        public OperandParseException(string message, Exception inner) : base(message, inner) { }
        protected OperandParseException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class TooManyArgumentOperandException : OperandParseException
    {
        public TooManyArgumentOperandException() { }
        public TooManyArgumentOperandException(string message) : base(message) { }
        public TooManyArgumentOperandException(string message, Exception inner) : base(message, inner) { }
        protected TooManyArgumentOperandException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class OperandConvertException : OperandParseException
    {
        public OperandConvertException() { }
        public OperandConvertException(string message) : base(message) { }
        public OperandConvertException(string message, Exception inner) : base(message, inner) { }
        protected OperandConvertException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

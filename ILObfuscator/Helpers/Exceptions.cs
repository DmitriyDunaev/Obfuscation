using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obfuscator
{
    [Serializable]
    public class ValidatorException : Exception
    {
        public ValidatorException() { }
        public ValidatorException(string message) : base(message) { }
        public ValidatorException(string message, Exception inner) : base(message, inner) { }
        protected ValidatorException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class ObfuscatorException : Exception
    {
        public ObfuscatorException() { }
        public ObfuscatorException(string message) : base(message) { }
        public ObfuscatorException(string message, Exception inner) : base(message, inner) { }
        protected ObfuscatorException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }


    [Serializable]
    public class RandomizerException : Exception
    {
        public RandomizerException() { }
        public RandomizerException(string message) : base(message) { }
        public RandomizerException(string message, Exception inner) : base(message, inner) { }
        protected RandomizerException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }


    [Serializable]
    public class ParserException : Exception
    {
        public ParserException() { }
        public ParserException(string message) : base(message) { }
        public ParserException(string message, Exception inner) : base(message, inner) { }
        protected ParserException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
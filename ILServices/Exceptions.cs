using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
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
    public class TraversalException : Exception
    {
        public TraversalException() { }
        public TraversalException(string message) : base(message) { }
        public TraversalException(string message, Exception inner) : base(message, inner) { }
        protected TraversalException(
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Objects
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
    public class ObjectException : Exception
    {
        public ObjectException() { }
        public ObjectException(string message) : base(message) { }
        public ObjectException(string message, Exception inner) : base(message, inner) { }
        protected ObjectException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}

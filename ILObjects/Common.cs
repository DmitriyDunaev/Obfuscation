using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Objects
{
    public static class Common
    {
        /// <summary>
        /// Memory region sizes for supported types
        /// </summary>
        public enum MemoryRegionSize
        {
            Integer = 4,
            Char = 1
        }

        public enum StatementType
        {
            FullAssignment = 0, // FullAssignment
            UnaryAssignment = 1, // UnaryAssignment
            Copy = 2, // Copy
            UnconditionalJump = 3, // UnconditionalJump
            ConditionalJump = 4, // ConditionalJump
            Procedural = 5, // Procedural
            IndexedAssignment = 6, // IndexedAssignment
            PointerAssignment = 7, // PointerAssignment
            NoOperation = 8 // NoOperation
        };

        public enum CalledFrom
        {
            InternalOnly = 0, // InternalOnly
            ExternalOnly = 1, // ExternalOnly
            Both = 2 // Both
        };


        /// <summary>
        /// Provides a full copy of original object
        /// </summary>
        /// <param name="obj">Original object to be copied</param>
        /// <returns>A stand-alone copy of original object</returns>
        public static object DeepClone(object obj)
        {
            object objResult = null;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bf.Serialize(ms, obj);
                ms.Position = 0;
                objResult = bf.Deserialize(ms);
            }
            return objResult;
        }
    }
}

using Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obfuscator
{
    public static class Common
    {
        // **** General Section ****
        
        /// <summary>
        /// Memory region sizes for supported types
        /// </summary>
        public enum MemoryRegionSize
        {
            Integer = 4,
            Char = 1
        }

        // **** Fake Code Generation Section ****
        
        /// <summary>
        /// Maximal number used in fake code generation (e.g. in conditional statements) 
        /// </summary>
        public static int GlobalMaxNumber = 1000;
        /// <summary>
        /// Minimal number used in fake code generation (e.g. in conditional statements) 
        /// </summary>
        public static int GlobalMinNumber = 10;
        /// <summary>
        /// Number of fake instructions to be generated per single original 
        /// </summary>
        public static int FPO = 5;
        /// <summary>
        /// Minimal number of instructions in a basic block
        /// </summary>
        public static int FakePadding = 20;
        /// <summary>
        /// Variability of fake_padding
        /// </summary>
        public static int FakePaddingVariance = 5;

        // **** Meshing Algorithm Section ****

        /// <summary>
        /// Number of the fake conditions that are generated in the conditional meshing algorithm
        /// </summary>
        public static int ConditionalJumpReplacementFactor = 6;
        /// <summary>
        /// The enumeration for the chances of the jump generation
        /// </summary>
        public enum JumpGenerationChances
        {
            Original = 20,
            Existing = 30,
            New = 50
        }
        

        // **** Common Methods ****

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

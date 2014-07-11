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
        /// Determines the maximal number of retrys, before the user is asked.
        /// </summary>
        public static int MaxNumberOfRuns = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["MaxNumberOfRuns"]);
        /// <summary>
        /// Percentage of local fake variables to be injected in relation to the number of original ones
        /// </summary>
        public static int PercentageFakeVars = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["PercentageFakeVars"]);
        
        /// <summary>
        /// Memory region sizes for supported types
        /// </summary>
        public enum MemoryRegionSize
        {
            Integer = 4,
            Char = 1
        }


        // **** Fake Parameters Section ****
        /// <summary>
        /// Number of fake input parameters to generate
        /// </summary>
        public static int NumFakeInputParam = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["NumFakeInputParam"]);
        /// <summary>
        /// Whether to push random VALID parameters or leave it to the user (Used for testing)
        /// </summary>
        public static bool RandomPushValues = Convert.ToBoolean(System.Configuration.ConfigurationSettings.AppSettings["RandomPushValues"]);

        // **** Meshing Parameters Section ****
        /// <summary>
        /// Whether to apply conditional meshing in fake conditional jumps
        /// </summary>             
        public static bool DoubleMeshing = Convert.ToBoolean(System.Configuration.ConfigurationSettings.AppSettings["DoubleMeshing"]);
        /// <summary>
        /// Probability of unconditional meshing in percents
        /// </summary>
        public static int UnconditionalMeshingProbability = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["UnconditionalMeshingProbability"]);
        /// <summary>
        /// Probability of conditional meshing in percents
        /// </summary>
        public static int ConditionalMeshingProbability = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["ConditionalMeshingProbability"]);
        /// <summary>
        /// Number of the fake conditions that are generated in the conditional meshing algorithm
        /// </summary>
        public static int ConditionalJumpRadius = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["ConditionalJumpRadius"]);
        /// <summary>
        /// Maximal range between the numbers used to define Loop conditional jumps
        /// </summary>
        public static int LoopConditionalJumpMaxRange = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["LoopConditionalJumpMaxRange"]);

        // **** Fake Code Generation Section ****

        /// <summary>
        /// Maximal number used in fake code generation (e.g. in conditional statements) 
        /// </summary>
        public static int GlobalMaxValue = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["GlobalMaxValue"]);
        /// <summary>
        /// Minimal number used in fake code generation (e.g. in conditional statements) 
        /// </summary>
        public static int GlobalMinValue = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["GlobalMinValue"]);
        /// <summary>
        /// Number of fake instructions to be generated per single original 
        /// </summary>
        public static int FPO = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["FPO"]);
        /// <summary>
        /// Minimal number of instructions in a basic block
        /// </summary>
        public static int FakePadding = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["FakePadding"]);
        /// <summary>
        /// Variability of fake_padding
        /// </summary>
        public static int FakePaddingVariance = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["FakePaddingVariance"]);
        /// <summary>
        /// Describes the probability of generating a conditional jump in percents.
        /// </summary>
        public static int ConditionalJumpProbability = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["ConditionalJumpProbability"]);
        /// <summary>
        /// Maximal number of basic blocks to jump back in order to create a infinite loop
        /// The greater the number, the higher is the chance to need more than one run to
        /// be able to create fake instructions correctly. This parameter will be used only
        /// when we don't have available basic blocks inside a loop body.
        /// </summary>
        public static int MaxJumpBackForLoop = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["MaxJumpBackForLoop"]);
        
        /// <summary>
        /// The enumeration for the chances of the jump generation
        /// </summary>
        public enum JumpGenerationChances
        {
            Original = 20,
            Existing = 30,
            New = 50
        }
        /// <summary>
        /// The direction at graph traversal
        /// </summary>
        public enum Direction
        {
            Up = 0,
            Down = 1
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

using Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Obfuscator
{
    public static class Common
    {
        // **** General Section ****
        /// <summary>
        /// Determines the maximal number of retrys, before the user is asked.
        /// </summary>
        public static int MaxNumberOfRuns = Convert.ToInt32(ConfigurationManager.AppSettings["MaxNumberOfRuns"]);
        /// <summary>
        /// Percentage of local fake variables to be injected in relation to the number of original ones
        /// </summary>
        public static int PercentageFakeVars = Convert.ToInt32(ConfigurationManager.AppSettings["PercentageFakeVars"]);
        

        // **** Fake Parameters Section ****
        /// <summary>
        /// Number of fake input parameters to generate
        /// </summary>
        public static int NumFakeInputParam = Convert.ToInt32(ConfigurationManager.AppSettings["NumFakeInputParam"]);


        // **** Meshing Parameters Section ****
        /// <summary>
        /// Whether to apply conditional meshing in fake conditional jumps
        /// </summary>             
        public static bool DoubleMeshing = Convert.ToBoolean(ConfigurationManager.AppSettings["DoubleMeshing"]);
        /// <summary>
        /// Probability of unconditional meshing in percents
        /// </summary>
        public static int UnconditionalMeshingProbability = Convert.ToInt32(ConfigurationManager.AppSettings["UnconditionalMeshingProbability"]);
        /// <summary>
        /// Probability of conditional meshing in percents
        /// </summary>
        public static int ConditionalMeshingProbability = Convert.ToInt32(ConfigurationManager.AppSettings["ConditionalMeshingProbability"]);
        /// <summary>
        /// Number of the fake conditions that are generated in the conditional meshing algorithm
        /// </summary>
        public static int ConditionalJumpRadius = Convert.ToInt32(ConfigurationManager.AppSettings["ConditionalJumpRadius"]);
        /// <summary>
        /// Maximal range between the numbers used to define Loop conditional jumps
        /// </summary>
        public static int LoopConditionalJumpMaxRange = Convert.ToInt32(ConfigurationManager.AppSettings["LoopConditionalJumpMaxRange"]);
        /// <summary>
        /// Probability of jumping into a loop body
        /// </summary>
        public static int JumpLoopBodyProbability = Convert.ToInt32(ConfigurationManager.AppSettings["JumpLoopBodyProbability"]);

        // **** Fake Code Generation Section ****

        /// <summary>
        /// Maximal number used in fake code generation (e.g. in conditional statements) 
        /// </summary>
        public static int GlobalMaxValue = Convert.ToInt32(ConfigurationManager.AppSettings["GlobalMaxValue"]);
        /// <summary>
        /// Minimal number used in fake code generation (e.g. in conditional statements) 
        /// </summary>
        public static int GlobalMinValue = Convert.ToInt32(ConfigurationManager.AppSettings["GlobalMinValue"]);
        /// <summary>
        /// Number of fake instructions to be generated per single original 
        /// </summary>
        public static int FPO = Convert.ToInt32(ConfigurationManager.AppSettings["FPO"]);
        /// <summary>
        /// Minimal number of instructions in a basic block
        /// </summary>
        public static int FakePadding = Convert.ToInt32(ConfigurationManager.AppSettings["FakePadding"]);
        /// <summary>
        /// Variability of fake_padding
        /// </summary>
        public static int FakePaddingVariance = Convert.ToInt32(ConfigurationManager.AppSettings["FakePaddingVariance"]);
        /// <summary>
        /// Describes the probability of generating a conditional jump in percents.
        /// </summary>
        public static int ConditionalJumpProbability = Convert.ToInt32(ConfigurationManager.AppSettings["ConditionalJumpProbability"]);
        /// <summary>
        /// Maximal number of basic blocks to jump back in order to create a infinite loop
        /// The greater the number, the higher is the chance to need more than one run to
        /// be able to create fake instructions correctly. This parameter will be used only
        /// when we don't have available basic blocks inside a loop body.
        /// </summary>
        public static int MaxJumpBackForLoop = Convert.ToInt32(ConfigurationManager.AppSettings["MaxJumpBackForLoop"]);
        /// <summary>
        /// Describes the probability of generating a new function in percent.
        /// </summary>
        public static int OutlingingProbability = Convert.ToInt32(ConfigurationManager.AppSettings["OutlingingProbability"]);
        /// <summary>
        /// Minimal number of instructions in a new function.
        /// </summary>
        public static int MaxInstructionsPerFunctions = Convert.ToInt32(ConfigurationManager.AppSettings["MaxInstructionsPerFunctions"]);

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
    }
}

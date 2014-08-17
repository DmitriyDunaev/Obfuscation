using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Objects;
using Services;

namespace Obfuscator
{
    public static class Meshing
    {

        /// <summary>
        /// The meshing algorithm, which will mesh the unconditional jumps in a routine
        /// </summary>
        /// <param name="rtn">The routine which will be meshed up</param>
        public static void MeshUnconditionals(Routine rtn)
        {
            /// Meshing of the unconditional jumps
            foreach (Function funct in rtn.Functions)
            {
                List<BasicBlock> basicblocks = funct.BasicBlocks.FindAll(x => x.Instructions.Last().statementType == Objects.Common.StatementType.UnconditionalJump);
                foreach (BasicBlock bb in basicblocks)
                {
                    int i = Randomizer.SingleNumber(0, 100);
                    if (i <= Common.UnconditionalMeshingProbability)
                    {
                        BasicBlock originalSuccessor = bb.getSuccessors.First();
                        BasicBlock falseLane = new BasicBlock(bb.parent);
                        //The false lane involves fake variables only in other to provided the correct output
                        falseLane.Involve = BasicBlock.InvolveInFakeCodeGeneration.FakeVariablesOnly;
                        falseLane.Meshable = false;
                        BasicBlock trueLane = new BasicBlock(bb.parent);
                        //The true lane should involve both fake and original variables in other to provided the wrong output
                        if (funct.containsDivisionModulo == false || originalSuccessor.Instructions.FindAll(x => x.TACtext.Contains("return")).Count > 0)
                            trueLane.Involve = BasicBlock.InvolveInFakeCodeGeneration.Both;
                        trueLane.Meshable = false;
                        trueLane.dead = true;
                        //Linking first the false lane
                        bb.LinkToSuccessor(falseLane, true);
                        //Creating the jump to the true lane
                        MakeInstruction.RandomConditionalJump(bb.Instructions.Last(), Instruction.ConditionType.AlwaysFalse, trueLane);
                        ExpandMainRoute(falseLane, originalSuccessor, false);
                        ExpandFakeRoute(trueLane, originalSuccessor, false);
                    }
                }
            }
        }

        /// <summary>
        /// The meshing algorithm, which will mesh the conditional jumps in a routine
        /// </summary>
        /// <param name="rtn">Thse routine which will be meshed up</param>
        public static void MeshConditionals(Routine rtn)
        {
            /// Meshing of the conditional jumps
            foreach (Function funct in rtn.Functions)
            {
                List<BasicBlock> basicblocks = funct.BasicBlocks.FindAll(x => x.Instructions.Last().statementType == Objects.Common.StatementType.ConditionalJump &&
                                                                            x.Instructions.Last().RefVariables.Count() == 1);
                foreach (BasicBlock bb in basicblocks)
                {
                    //We perform the conditonal meshing if
                    //1) we have DoubleMeshing
                    //2) we don't have DoubleMeshing but the basic block is meshable (only original basic blocks)
                    if (Common.DoubleMeshing || (Common.DoubleMeshing == false && bb.Meshable))
                    {
                        int i = Randomizer.SingleNumber(0, 100);
                        if (i <= Common.ConditionalMeshingProbability)
                        {
                            InsertConditionals(bb);
                        }
                    }
                }
            }            
        }

        /// <summary>
        /// Expands the Main Route into the CFG, creating the conditional jump for both Main Route and Fake Route
        /// </summary>
        /// <param name="fake1">The first fake basic block to be used in the expansion</param>
        /// <param name="mainLaneBB">The original basic block we should jump to stay in the Main Route</param>
        /// <param name="atFakeCodeGeneration">Wheter we are or not at the fake code generation phase</param>
        private static void ExpandMainRoute(BasicBlock fake1, BasicBlock originalSuccessor, bool atFakeCodeGeneration)
        {
            //Fetching the Conditional Jump (AlwaysFalse) created in the previous basic block
            Instruction originalInstruction = fake1.getPredecessors.First().Instructions.Last();

            //Defining the constant and relational operator for the fake1 conditional jump 
            Instruction.RelationalOperationType relationalOperation;
            int rightValue = 0;
            if (originalInstruction.GetConstFromCondition() >= originalInstruction.GetVarFromCondition().fixedMax.Value)
            {
                //If the variable's fixed minimun is diferent from the global min plus the maximun loop range, we should try
                //to get a value different from the variable's fixed minimun in order to have a more belieavable code
                do
                {
                    rightValue = Randomizer.SingleNumber(Common.GlobalMinValue + Common.LoopConditionalJumpMaxRange,
                        originalInstruction.GetVarFromCondition().fixedMin.Value);
                } while (rightValue == originalInstruction.GetVarFromCondition().fixedMin.Value &&
                originalInstruction.GetVarFromCondition().fixedMin.Value != Common.GlobalMinValue + Common.LoopConditionalJumpMaxRange);

                relationalOperation = (Instruction.RelationalOperationType)
                                                        Randomizer.OneFromMany(Instruction.RelationalOperationType.Smaller,
                                                                            Instruction.RelationalOperationType.SmallerOrEquals);
            }
            else
            {
                //If the variable's fixed maximun is diferent from the global max minus the maximun loop range, we should try
                //to get a value different from the variable's fixed maximun in order to have a more belieavable code
                do
                {
                    rightValue = Randomizer.SingleNumber(originalInstruction.GetVarFromCondition().fixedMax.Value,
                        Common.GlobalMaxValue - Common.LoopConditionalJumpMaxRange);
                } while (rightValue == originalInstruction.GetVarFromCondition().fixedMax.Value &&
                originalInstruction.GetVarFromCondition().fixedMax.Value != Common.GlobalMaxValue - Common.LoopConditionalJumpMaxRange);

                relationalOperation = (Instruction.RelationalOperationType)
                                                        Randomizer.OneFromMany(Instruction.RelationalOperationType.Greater,
                                                                            Instruction.RelationalOperationType.GreaterOrEquals);
            }

            //Creating and expanding the false lane that stays in the Main Route
            fake1.LinkToSuccessor(ExpandMainRoute(originalSuccessor), true);

            //Creating fake2 to hold the true lane for the conditional jump
            BasicBlock fake2 = new BasicBlock(fake1.parent);
            fake2.Meshable = false;
            if (fake2.parent.containsDivisionModulo == false || originalSuccessor.Instructions.FindAll(x => x.TACtext.Contains("return")).Count > 0)
                fake2.Involve = BasicBlock.InvolveInFakeCodeGeneration.Both;

            //Creating the conditional jump (always false) to fake route
            Variable var = originalInstruction.GetVarFromCondition();
            MakeInstruction.ConditionalJump(fake1.Instructions.Last(), var, rightValue, relationalOperation, fake2); 
            //Expanding the fake route
            ExpandFakeRoute(fake2, originalSuccessor, atFakeCodeGeneration);            
        }

        /// <summary>
        /// Expands the Main Route into the CFG, inserting "FakeVariablesOnly" basic blocks and random conditional jump
        /// between them
        /// </summary>
        /// <param name="originaltarget">The basic block we should go to stay in the Main Route</param>
        private static BasicBlock ExpandMainRoute(BasicBlock originaltarget)
        {
            // Creating a new basic block
            BasicBlock fake1 = new BasicBlock(originaltarget.parent);
            fake1.Instructions.Add(new Instruction(fake1));
            fake1.Meshable = false;

            // Creating the second fake block
            BasicBlock fake2 = new BasicBlock(originaltarget.parent);
            fake2.Instructions.Add(new Instruction(fake2));

            // Starting the linking
            MakeInstruction.UnconditionalJump(fake1.Instructions.Last(), fake2);

            BasicBlock polyrequtarget = null;

            // Creating a clone of the original target in order to make the CFT more obfuscated
            if (originaltarget.Instructions.Last().statementType == Objects.Common.StatementType.UnconditionalJump)
                polyrequtarget = new BasicBlock(originaltarget, originaltarget.getSuccessors);
            else 
                polyrequtarget = originaltarget;

            // And now setting the edges
            MakeInstruction.UnconditionalJump(fake2.Instructions.Last(), polyrequtarget);

            // And then converting its nop instruction into a ConditionalJump
            MakeInstruction.RandomConditionalJump(fake1.Instructions.Last(), Instruction.ConditionType.Random, originaltarget);

            return fake1;

        }

        /// <summary>
        /// Expands the fake route into the CFG and creates the conditional jumps for both Loop and Fake code
        /// </summary>
        /// <param name="fake1">The first fake basic block to be used in the expansion</param>
        /// <param name="mainLaneBB">The basic block we should go in case of no-loop</param>
        /// <param name="atFakeCodeGeneration">Wheter we are or not at the fake code generation phase</param>
        public static void ExpandFakeRoute(BasicBlock fake1, BasicBlock originalSuccessor, bool atFakeJumpGeneration)
        {
            //Creating fake2 to hold the next Loop condition
            BasicBlock fake2 = new BasicBlock(fake1.parent);
            fake2.Meshable = false;
            if (fake2.parent.containsDivisionModulo == false 
                || originalSuccessor.Instructions.FindAll(x => x.TACtext.Contains("return")).Count > 0)
                fake2.Involve = BasicBlock.InvolveInFakeCodeGeneration.Both;
            fake2.dead = true;

            //Creating fake3 to hold the extra fake code in case of No-Loop
            BasicBlock fake3 = new BasicBlock(fake1.parent);
            if (fake3.parent.containsDivisionModulo == false 
                || originalSuccessor.Instructions.FindAll(x => x.TACtext.Contains("return")).Count > 0)
                fake3.Involve = BasicBlock.InvolveInFakeCodeGeneration.Both;
            fake3.dead = true;

            //Creating the fake3 unconditional jump back to the Main Lane
            MakeInstruction.UnconditionalJump(fake3.Instructions.Last(), originalSuccessor);

            //Linking fake3 to fake2
            fake2.LinkToSuccessor(fake3);

            //Creating fake4 to hold the extra fake code in case of No-Loop
            BasicBlock fake4 = new BasicBlock(fake1.parent);
            if (fake4.parent.containsDivisionModulo == false 
                || originalSuccessor.Instructions.FindAll(x => x.TACtext.Contains("return")).Count > 0)
                fake4.Involve = BasicBlock.InvolveInFakeCodeGeneration.Both;
            fake4.dead = true;

            //Creating the fake4 unconditional jump back to the Main Lane
            MakeInstruction.UnconditionalJump(fake4.Instructions.Last(), originalSuccessor);

            //Linking fake4 to fake1
            fake1.LinkToSuccessor(fake4);

            //Fetching the Conditional Jump (AlwaysFalse) created in the previous basic block
            Instruction originalInstruction = fake1.getPredecessors.First().Instructions.Last();

            //Defining the relational operator for the fake1 conditional jump
            Instruction.RelationalOperationType relationalOperationEF1 = (Instruction.RelationalOperationType)
                                                        Randomizer.OneFromMany(Instruction.RelationalOperationType.Smaller,
                                                                            Instruction.RelationalOperationType.SmallerOrEquals,
                                                                            Instruction.RelationalOperationType.Greater,
                                                                            Instruction.RelationalOperationType.GreaterOrEquals);

            //Defining the constant for the fake1 conditional jump
            int rightValueEF1 = 0;
            if (originalInstruction.GetConstFromCondition() >= originalInstruction.GetVarFromCondition().fixedMax.Value)
            {
                //If the original instruction's contant is diferent from the global max minus the max loop range, we should try
                //to get a value different from the original instruction's constant in order to have a more belieavable code
                do
                {
                    rightValueEF1 = Randomizer.SingleNumber((int)originalInstruction.GetConstFromCondition(),
                        Common.GlobalMaxValue - Common.LoopConditionalJumpMaxRange);
                } while (rightValueEF1 == (int)originalInstruction.GetConstFromCondition() &&
                (int)originalInstruction.GetConstFromCondition() != Common.GlobalMaxValue - Common.LoopConditionalJumpMaxRange);
            }
            else
            {
                //If the original instruction's contant is diferent from the global min plus the max loop range, we should try
                //to get a value different from the original instruction's constant in order to have a more belieavable code
                do
                {
                    rightValueEF1 = Randomizer.SingleNumber(Common.GlobalMinValue + Common.LoopConditionalJumpMaxRange,
                        (int)originalInstruction.GetConstFromCondition());
                } while (rightValueEF1 == (int)originalInstruction.GetConstFromCondition() &&
                 (int)originalInstruction.GetConstFromCondition() != Common.GlobalMinValue + Common.LoopConditionalJumpMaxRange);
            }
            //Creating the fake1 conditional jump
            Variable var = originalInstruction.GetVarFromCondition();
            MakeInstruction.ConditionalJump(fake1.Instructions.Last(), var, rightValueEF1, relationalOperationEF1, fake2);

            //Defining the range for the fake2 conditional jump constant
            int range = Randomizer.SingleNumber(0, Common.LoopConditionalJumpMaxRange);

            //Defining relational operation for the extraFake2 conditional jump based on the extraFake1 condtional jump relational operation and the range
            Instruction.RelationalOperationType relationalOperationEF2 = Instruction.RelationalOperationType.Equals;
            switch (relationalOperationEF1)
            {
                case Instruction.RelationalOperationType.Smaller:
                case Instruction.RelationalOperationType.SmallerOrEquals:
                    if (range <= 1)
                        relationalOperationEF2 = Instruction.RelationalOperationType.Equals;
                    else if (range == 2)
                        relationalOperationEF2 = (Instruction.RelationalOperationType)
                                                        Randomizer.OneFromMany(Instruction.RelationalOperationType.GreaterOrEquals,
                                                                            Instruction.RelationalOperationType.Equals);
                    else
                        relationalOperationEF2 = (Instruction.RelationalOperationType)
                                                        Randomizer.OneFromMany(Instruction.RelationalOperationType.Greater,
                                                                            Instruction.RelationalOperationType.GreaterOrEquals,
                                                                            Instruction.RelationalOperationType.Equals);
                    break;
                case Instruction.RelationalOperationType.Greater:
                case Instruction.RelationalOperationType.GreaterOrEquals:
                    if (range <= 1)
                        relationalOperationEF2 = Instruction.RelationalOperationType.Equals;
                    else if (range == 2)
                        relationalOperationEF2 = (Instruction.RelationalOperationType)
                                                        Randomizer.OneFromMany(Instruction.RelationalOperationType.SmallerOrEquals,
                                                                            Instruction.RelationalOperationType.Equals);
                    else
                        relationalOperationEF2 = (Instruction.RelationalOperationType)
                                                Randomizer.OneFromMany(Instruction.RelationalOperationType.Smaller,
                                                                    Instruction.RelationalOperationType.SmallerOrEquals,
                                                                    Instruction.RelationalOperationType.Equals);
                    break;
            }

            //Defining the constant for the fake2 conditional jump
            int rightValueEF2 = 0;
            if (rightValueEF1 == Common.GlobalMinValue || rightValueEF1 == Common.GlobalMaxValue)
                rightValueEF2 = rightValueEF1;
            else
            {
                switch (relationalOperationEF2)
                {
                    case Instruction.RelationalOperationType.Smaller:
                    case Instruction.RelationalOperationType.SmallerOrEquals:
                        if (rightValueEF1 + range > Common.GlobalMaxValue)
                            rightValueEF2 = Randomizer.SingleNumber(rightValueEF1, Common.GlobalMaxValue);
                        else if (rightValueEF1 + range > (int)originalInstruction.GetConstFromCondition() &&
                            (originalInstruction.GetRelopFromCondition() == Instruction.RelationalOperationType.Smaller
                            || originalInstruction.GetRelopFromCondition() == Instruction.RelationalOperationType.SmallerOrEquals))
                            rightValueEF2 = Randomizer.SingleNumber(rightValueEF1, (int)originalInstruction.GetConstFromCondition());
                        else 
                            rightValueEF2 = Randomizer.SingleNumber(rightValueEF1, rightValueEF1 + range);
                        break;
                    case Instruction.RelationalOperationType.Greater:
                    case Instruction.RelationalOperationType.GreaterOrEquals:
                        if (rightValueEF1 - range < Common.GlobalMinValue)
                            rightValueEF2 = Randomizer.SingleNumber(Common.GlobalMinValue, rightValueEF1);
                        else if (rightValueEF1 - range < (int)originalInstruction.GetConstFromCondition()&&
                            (originalInstruction.GetRelopFromCondition() == Instruction.RelationalOperationType.Greater
                            || originalInstruction.GetRelopFromCondition() == Instruction.RelationalOperationType.GreaterOrEquals))
                            rightValueEF2 = Randomizer.SingleNumber((int)originalInstruction.GetConstFromCondition(), rightValueEF1);
                        else
                            rightValueEF2 = Randomizer.SingleNumber(rightValueEF1 - range, rightValueEF1);
                        break;
                    case Instruction.RelationalOperationType.Equals:
                        {
                            switch (relationalOperationEF1)
                            {
                                case Instruction.RelationalOperationType.Smaller:
                                case Instruction.RelationalOperationType.SmallerOrEquals:
                                    if (rightValueEF1 - range < Common.GlobalMinValue)
                                        rightValueEF2 = Randomizer.SingleNumber(Common.GlobalMinValue, rightValueEF1);
                                    else if (rightValueEF1 - range < (int)originalInstruction.GetConstFromCondition() &&
                                        (originalInstruction.GetRelopFromCondition() == Instruction.RelationalOperationType.Greater
                                        || originalInstruction.GetRelopFromCondition() == Instruction.RelationalOperationType.GreaterOrEquals))
                                        rightValueEF2 = Randomizer.SingleNumber((int)originalInstruction.GetConstFromCondition(), rightValueEF1);
                                    else
                                        rightValueEF2 = Randomizer.SingleNumber(rightValueEF1 - range, rightValueEF1);
                                    break;
                                case Instruction.RelationalOperationType.Greater:
                                case Instruction.RelationalOperationType.GreaterOrEquals:
                                    if (rightValueEF1 + range > Common.GlobalMaxValue)
                                        rightValueEF2 = Randomizer.SingleNumber(rightValueEF1, Common.GlobalMaxValue);
                                    else if (rightValueEF1 + range > (int)originalInstruction.GetConstFromCondition() &&
                                        (originalInstruction.GetRelopFromCondition() == Instruction.RelationalOperationType.Smaller
                                        ||originalInstruction.GetRelopFromCondition() == Instruction.RelationalOperationType.SmallerOrEquals))
                                        rightValueEF2 = Randomizer.SingleNumber(rightValueEF1, (int)originalInstruction.GetConstFromCondition());
                                    else
                                        rightValueEF2 = Randomizer.SingleNumber(rightValueEF1, rightValueEF1 + range);
                                    break;
                            }
                            break;
                        }
                }
            }

            //Selecting the target for fake2 in order to create a loop
            List<BasicBlock> reacheableBasicBlocks = DataAnalysis.GetReachableBasicBlocks(fake2, Common.Direction.Up);
            //In case we are at fake jump generation, we should try to choose reachable basic blocks in a loop body
            //in order to make the CFG irreducible
            if (atFakeJumpGeneration)
            {
                List<BasicBlock> loopReachableBasicBlocks = reacheableBasicBlocks.FindAll(x => DataAnalysis.isLoopBody.Keys.Contains(x)
                    && DataAnalysis.isLoopBody[x] == true);
                //If we have reachable basic blocks in a loop body, we can use them
                if (loopReachableBasicBlocks.Count > 0)
                {
                    int i = Randomizer.SingleNumber(0, 100);
                    if (!fake2.parent.irreducibleCFG || i <= Common.JumpLoopBodyProbability)
                    {
                        reacheableBasicBlocks = loopReachableBasicBlocks;
                        fake2.parent.irreducibleCFG = true;
                    }
                }
            }
            //Check whether the amount of reachable basic blocks we have is greater than MaxJumpForLoop. 
            //This parameter is used to control the chance of problems ("nops" left in the end) during 
            //fake instructions generation.
            if (reacheableBasicBlocks.Count > Common.MaxJumpBackForLoop)
                reacheableBasicBlocks.RemoveRange(Common.MaxJumpBackForLoop, reacheableBasicBlocks.Count - Common.MaxJumpBackForLoop);

            //Defining the target basic block for the conditional jump
            BasicBlock loopTarget = (BasicBlock) Randomizer.OneFromMany(reacheableBasicBlocks.ToArray());

            //Creating the fake2 conditional jump to the loop target
            MakeInstruction.ConditionalJump(fake2.Instructions.Last(), var, rightValueEF2, relationalOperationEF2, loopTarget);
        }

        /// <summary>
        /// Class of constants with the relational operations available list
        /// </summary>
        public class Cond
        {
            public enum BlockJumpType
            {
                True,
                False,
                Ambigious,
                Last
            }

            /// <summary>
            /// The constant value of the condition
            /// </summary>
            public int value;

            /// <summary>
            /// The relational operator of the condition
            /// </summary>
            public Instruction.RelationalOperationType relop;

            /// <summary>
            /// The jump type of the unconditional jump, that will be generated from this condition
            /// </summary>
            public BlockJumpType JumpType;

            /// <summary>
            /// Constructor to the condition class
            /// </summary>
            /// <param name="generatedconstant">The generated constant of the condition</param>
            /// <param name="originalrelop">The original relational operator</param>
            /// <param name="originalconstant">The original constant</param>
            public Cond(int generatedconstant, Instruction.RelationalOperationType originalrelop, int originalconstant)
            {
                value = generatedconstant;
                SetJumpType(generatedconstant, originalconstant, originalrelop);
            }

            /// <summary>
            /// Sets the jump type of a Cond
            /// </summary>
            /// <param name="generatedconstant">The constant which has been generated by us</param>
            /// <param name="originalconstant">The original constant</param>
            /// <param name="originalrelop">The original relational operator</param>
            private void SetJumpType(int generatedconstant, int originalconstant, Instruction.RelationalOperationType originalrelop)
            {
                if (generatedconstant == originalconstant - 1)
                {
                    switch (originalrelop)
                    {
                        case Instruction.RelationalOperationType.GreaterOrEquals:
                            relop = Instruction.RelationalOperationType.Greater;
                            JumpType = BlockJumpType.Last;
                            break;
                        case Instruction.RelationalOperationType.Smaller:
                            relop = Instruction.RelationalOperationType.SmallerOrEquals;
                            JumpType = BlockJumpType.Last;
                            break;
                        case Instruction.RelationalOperationType.Equals:
                            relop = Instruction.RelationalOperationType.Greater;
                            JumpType = BlockJumpType.Last;
                            break;
                        case Instruction.RelationalOperationType.NotEquals:
                            relop = Instruction.RelationalOperationType.Smaller;
                            JumpType = BlockJumpType.Last;
                            break;
                        default:
                            LessPattern(originalrelop);
                            break;
                    }
                }
                else if (generatedconstant == originalconstant + 1)
                {
                    switch (originalrelop)
                    {
                        case Instruction.RelationalOperationType.Greater:
                            relop = Instruction.RelationalOperationType.GreaterOrEquals;
                            JumpType = BlockJumpType.Last;
                            break;
                        case Instruction.RelationalOperationType.SmallerOrEquals:
                            relop = Instruction.RelationalOperationType.Smaller;
                            JumpType = BlockJumpType.Last;
                            break;
                        case Instruction.RelationalOperationType.Equals:
                            relop = Instruction.RelationalOperationType.Smaller;
                            JumpType = BlockJumpType.Last;
                            break;
                        case Instruction.RelationalOperationType.NotEquals:
                            relop = Instruction.RelationalOperationType.Greater;
                            JumpType = BlockJumpType.Last;
                            break;
                        default:
                            GreaterPattern(originalrelop);
                            break;
                    }
                }
                else if (generatedconstant < originalconstant)
                {
                    LessPattern(originalrelop);
                }
                else
                {
                    GreaterPattern(originalrelop);
                }
            }

            /// <summary>
            /// Sets the jump type of the Cond, based on the original relational operator, and a pattern
            /// </summary>
            /// <param name="originalrelop">The original relational operator</param>
            private void GreaterPattern(Instruction.RelationalOperationType originalrelop)
            {
                relop = (Instruction.RelationalOperationType)Randomizer.OneFromMany(Instruction.RelationalOperationType.Equals,
                                                                                    Instruction.RelationalOperationType.Greater,
                                                                                    Instruction.RelationalOperationType.GreaterOrEquals,
                                                                                    Instruction.RelationalOperationType.NotEquals,
                                                                                    Instruction.RelationalOperationType.Smaller,
                                                                                    Instruction.RelationalOperationType.SmallerOrEquals);
                if (relop == Instruction.RelationalOperationType.Greater || relop == Instruction.RelationalOperationType.GreaterOrEquals || relop == Instruction.RelationalOperationType.Equals)
                {
                    if (originalrelop == Instruction.RelationalOperationType.Greater || originalrelop == Instruction.RelationalOperationType.GreaterOrEquals || originalrelop == Instruction.RelationalOperationType.NotEquals)
                        JumpType = BlockJumpType.True;
                    else JumpType = BlockJumpType.False;
                }
                else JumpType = BlockJumpType.Ambigious;
            }

            /// <summary>
            /// Sets the jump type of the Cond, based on the original relational operator, and a pattern
            /// </summary>
            /// <param name="originalrelop">The original relational operator</param>
            private void LessPattern(Instruction.RelationalOperationType originalrelop)
            {
                relop = (Instruction.RelationalOperationType)Randomizer.OneFromMany(Instruction.RelationalOperationType.Equals,
                                                                                    Instruction.RelationalOperationType.Greater,
                                                                                    Instruction.RelationalOperationType.GreaterOrEquals,
                                                                                    Instruction.RelationalOperationType.NotEquals,
                                                                                    Instruction.RelationalOperationType.Smaller,
                                                                                    Instruction.RelationalOperationType.SmallerOrEquals);
                if (relop == Instruction.RelationalOperationType.Smaller || relop == Instruction.RelationalOperationType.SmallerOrEquals || relop == Instruction.RelationalOperationType.Equals)
                {
                    if (originalrelop == Instruction.RelationalOperationType.Greater || originalrelop == Instruction.RelationalOperationType.GreaterOrEquals || originalrelop == Instruction.RelationalOperationType.Equals)
                        JumpType = BlockJumpType.False;
                    else JumpType = BlockJumpType.True;
                }
                else JumpType = BlockJumpType.Ambigious;
            }
        }

        /// <summary>
        /// This function meshes up a conditional jump
        /// </summary>
        /// <param name="bb">The block containing the conditional jump to mesh up</param>
        private static void InsertConditionals(BasicBlock bb)
        {
            Variable var = null, tmp;
            Instruction.RelationalOperationType relop = 0;
            int? C = 0;
            BasicBlock truesucc = null;
            BasicBlock falsesucc = null;
            Parser.ConditionalJump(bb.Instructions.Last(), out var, out tmp, out C, out relop, out truesucc, out falsesucc);
            List<Cond> condlist = GenetateCondList((int)C, relop);
            GenerateBlocks(bb, var, truesucc, falsesucc, condlist, (int)C, relop);
        }

        /// <summary>
        /// This function generates the BasicBlocks into the function, based on the condition list
        /// </summary>
        /// <param name="bb">The actual basicblock with the conditional jump at the end</param>
        /// <param name="truelane">The true successor</param>
        /// <param name="falselane">The false successor</param>
        /// <param name="condlist">The condition list</param>
        /// <returns>The list of the generated BasicBlocks</returns>
        private static void GenerateBlocks(BasicBlock bb, Variable var, BasicBlock truelane, BasicBlock falselane, List<Cond> condlist, int originalconstant, Instruction.RelationalOperationType originalrelop)
        {
            List<BasicBlock> bblist = new List<BasicBlock>();
            foreach (Cond c in condlist)
            {
                bblist.Add(new BasicBlock(bb.parent));
                bblist.Last().Instructions.Add(new Instruction(bblist.Last()));
            }

            bb.Instructions.Remove(bb.Instructions.Last());
            bb.Instructions.Add(new Instruction(bb));

            /// Replacing the first one, with the original
            bb.parent.BasicBlocks.Remove(bblist.First());
            bblist.Remove(bblist.First());
            bblist.Insert(0, bb);

            /// Creating the polyRequired list
            List<BasicBlock> truelist = new List<BasicBlock>();
            truelist.Add(truelane);
            List<BasicBlock> falselist = new List<BasicBlock>();
            falselist.Add(falselane);

            for (int i = 0; i < condlist.Count(); i++)
            {
                switch (condlist[i].JumpType)
                {
                    case Cond.BlockJumpType.True:
                    case Cond.BlockJumpType.Last:
                        if (i != condlist.Count() - 1)
                            bblist[i].LinkToSuccessor(bblist[i + 1], true);
                        else
                            bblist[i].LinkToSuccessor(GeneratePolyReqJumpTarget(falselist), true);
                        MakeInstruction.ConditionalJump(bblist[i].Instructions.Last(), var, condlist[i].value, condlist[i].relop, GeneratePolyReqJumpTarget(truelist));
                        break;
                    case Cond.BlockJumpType.False:
                        if (i != condlist.Count() - 1)
                            bblist[i].LinkToSuccessor(bblist[i + 1], true);
                        else
                            bblist[i].LinkToSuccessor(GeneratePolyReqJumpTarget(falselist), true);
                        MakeInstruction.ConditionalJump(bblist[i].Instructions.Last(), var, condlist[i].value, condlist[i].relop, GeneratePolyReqJumpTarget(falselist));
                        break;
                    case Cond.BlockJumpType.Ambigious:
                        BasicBlock ambhelper = new BasicBlock(bb.parent);
                        bblist[i].LinkToSuccessor(ambhelper, true);
                        ambhelper.Instructions.Add(new Instruction(ambhelper));
                        if ((condlist[i].value + 1 == originalconstant || condlist[i].value - 1 == originalconstant) &&
                             originalrelop == Instruction.RelationalOperationType.NotEquals)
                        {
                            MakeInstruction.UnconditionalJump(ambhelper.Instructions.Last(), GeneratePolyReqJumpTarget(bblist.GetRange(i + 1, bblist.Count - (i + 1))));
                            MakeInstruction.ConditionalJump(bblist[i].Instructions.Last(), var, condlist[i].value, condlist[i].relop, truelist.First());
                        }
                        else
                        {
                            if ((originalconstant > condlist[i].value && (condlist[i].relop == Instruction.RelationalOperationType.Greater ||
                                                                            condlist[i].relop == Instruction.RelationalOperationType.GreaterOrEquals) &&
                                                                            (originalrelop == Instruction.RelationalOperationType.Greater ||
                                    originalrelop == Instruction.RelationalOperationType.GreaterOrEquals)) ||
                                (originalconstant < condlist[i].value && (condlist[i].relop == Instruction.RelationalOperationType.Smaller ||
                                                                            condlist[i].relop == Instruction.RelationalOperationType.SmallerOrEquals) &&
                                                                            (originalrelop == Instruction.RelationalOperationType.Smaller ||
                                    originalrelop == Instruction.RelationalOperationType.SmallerOrEquals)) ||
                                (originalconstant < condlist[i].value && (originalrelop == Instruction.RelationalOperationType.Smaller ||
                                    originalrelop == Instruction.RelationalOperationType.SmallerOrEquals) &&
                                    (condlist[i].relop == Instruction.RelationalOperationType.NotEquals)) ||
                                (originalconstant > condlist[i].value && (originalrelop == Instruction.RelationalOperationType.Greater ||
                                    originalrelop == Instruction.RelationalOperationType.GreaterOrEquals) &&
                                    (condlist[i].relop == Instruction.RelationalOperationType.NotEquals)))
                                MakeInstruction.UnconditionalJump(ambhelper.Instructions.Last(), GeneratePolyReqJumpTarget(falselist));
                            else
                                MakeInstruction.UnconditionalJump(ambhelper.Instructions.Last(), GeneratePolyReqJumpTarget(truelist));
                            MakeInstruction.ConditionalJump(bblist[i].Instructions.Last(), var, condlist[i].value, condlist[i].relop, bblist[i + 1]);
                        }
                        break;
                    default:
                        break;
                }
            }
            /// Checking if there is a jump to the original targets, and if there is not, we can delete that
            /// BasicBlock, there must be a polyRequired clone of that.
            if (falselane.getPredecessors.Count() == 0)
            {
                foreach (BasicBlock succ in falselane.getSuccessors)
                {
                    succ.getPredecessors.Remove(falselane);
                }
                falselane.parent.BasicBlocks.Remove(falselane);
            }
            if (truelane.getPredecessors.Count() == 0)
            {
                foreach (BasicBlock succ in truelane.getSuccessors)
                {
                    succ.getPredecessors.Remove(truelane);
                }
                truelane.parent.BasicBlocks.Remove(truelane);
            }
        }

        /// <summary>
        /// This function repositions the Cond or Conds with JumpType Last behind the last of the ambigous ones in the list
        /// </summary>
        /// <param name="condlist">A list of Conds to reorganize</param>
        private static void RepositionLasts(List<Cond> condlist)
        {
            
            List<Cond> lasts = GetLasts(condlist);
            if (lasts.Count() == 2) lasts.First().JumpType = Cond.BlockJumpType.Ambigious;
            foreach (Cond c in lasts)
            {
                condlist.Remove(c);
            }
            int lastambigous = FindLastAmb(condlist);
            condlist.InsertRange(lastambigous + 1, lasts);
        }

        /// <summary>
        /// Finds the Conds which has JumpType Last
        /// </summary>
        /// <param name="condlist">The list of the Conds</param>
        /// <returns>A list of the required Conds</returns>
        private static List<Cond> GetLasts(List<Cond> condlist)
        {
            List<Cond> returnlist = new List<Cond>();
            foreach (Cond c in condlist)
            {
                if (c.JumpType == Cond.BlockJumpType.Last)
                {
                    returnlist.Add(c);
                }
            }
            return returnlist;
        }

        /// <summary>
        /// Finds the last ambigus Cond in a list
        /// </summary>
        /// <param name="constlist">The list to find the Cond in</param>
        /// <returns>The index of the last ambigous Cond, or if no such, the last Cond</returns>
        private static int FindLastAmb(List<Cond> constlist)
        {
            int ret = constlist.Count() -1;
            for (int i = 0; i < constlist.Count(); i++)
            {
                if (constlist[i].JumpType == Cond.BlockJumpType.Ambigious)
                    ret = i;
            }
            return ret;
        }
        

        /// <summary>
        /// This function sets the target of a jump from a conditional jump
        /// The result can be: the original target, one of the generated
        /// polyRequired targets, or a new polyRequired target
        /// </summary>
        /// <param name="targetlist">The already existing list, with the original jump target at its first position</param>
        /// <param name="generatenew">If thue, dispite the chances we generate a new target</param>
        /// <returns>The target of the jump, based on some parameters in the Common static class</returns>
        public static BasicBlock GeneratePolyReqJumpTarget(List<BasicBlock> targetlist)
        {
            if (targetlist.First().Instructions.Last().statementType != Objects.Common.StatementType.UnconditionalJump)
            {
                return targetlist.First();
            }
            Common.JumpGenerationChances result = (Common.JumpGenerationChances)Randomizer.OneFromManyWithProbability(new int[3] {    (int)Common.JumpGenerationChances.Original,
                                                                                                                            (int)Common.JumpGenerationChances.Existing,
                                                                                                                            (int)Common.JumpGenerationChances.New },
                                                                                                            Common.JumpGenerationChances.Original,
                                                                                                            Common.JumpGenerationChances.Existing,
                                                                                                            Common.JumpGenerationChances.New);
            if (result == Common.JumpGenerationChances.Original)
            {
                return targetlist.First();
            }
            else if (result == Common.JumpGenerationChances.Existing)
            {
                if (targetlist.Count() == 1) return targetlist.First();
                return targetlist[Randomizer.SingleNumber(1, targetlist.Count() - 1)];
            }
            else if (result == Common.JumpGenerationChances.New)
            {
                targetlist.Add(new BasicBlock(targetlist.First(), targetlist.First().getSuccessors));
                return targetlist.Last();
            }
            else
                return null;
        }


        /// <summary>
        /// Generates the surrounding CondConstants around a specific constant
        /// </summary>
        /// <param name="originalconstant">The actual constant which is the base of the generation</param>
        /// <param name="originalrelop">The original relational operator</param>
        /// <returns>The list of generated CondConstants</returns>
        private static List<Cond> GenetateCondList(int originalconstant, Instruction.RelationalOperationType originalrelop)
        {
            int num = Common.ConditionalJumpRadius;
            List<Cond> returnlist = new List<Cond>();
            for (int i = -1, n = 0; n < num; n++)
            {

                returnlist.Add(new Cond(originalconstant + i, originalrelop, originalconstant));

                if (i > 0) 
                    i++;
                else if (i < -1) 
                    i--;
                i *= -1;
            }
            returnlist.Shuffle<Cond>();
            RepositionLasts(returnlist);
            return returnlist;
        }
    }
}

﻿
/*
 * So this is the algorithm for meshing the control flow transition blocks.
 * This algorithm can be divided into two main parts:
 *  - Meshing the unconditional jumps
 *  - Meshing the conditional jumps
 * 
 * The complexity of the final CFG is based on the CFT Ratio, or Control Flow Transition
 * Ratio, which is the basic parameter of this algorithm of the obfuscaion. Depending on
 * this, the run time of the obfuscated part obviously slow down, so further considerations
 * must be done to set the default value of the CFT Ratio.
 * 
 * First of all, the algorithm needs a set of jumps -- conditional and unconditional also
 * -- on which the meshing will be terminated. The setting of this set is the first step,
 * because if we mesh up the CFG based on the unconditional jumps, and we try to determine
 * the conditional jumps which will locate the place of the meshing by the conditional
 * jumps, we might face the situation that for instance we only mesh up the conditional
 * jumps which are generated by us erstwhile, the original conditional jumps will remain
 * unmeshed... this explanation is getting to be too verbosed, so the point is: the first
 * step is to define the several jumps which the mesh will be based on.
 */

// Dmitriy: Agree on that, but we can easily solve it. Let's discuss it on Friday!


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Internal;

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
                List<BasicBlock> basicblocks = funct.BasicBlocks.FindAll(x => x.Instructions.Last().statementType == ExchangeFormat.StatementTypeType.EnumValues.eUnconditionalJump);
                foreach (BasicBlock bb in basicblocks)
                {
                    BasicBlock mainRouteBB = bb.getSuccessors.First();
                    BasicBlock fake = new BasicBlock(bb.parent);
                    BasicBlock extraFake = new BasicBlock(bb.parent);
                    extraFake.dead = true;
                    bb.LinkToSuccessor(fake, true);
                    Randomizer.GenerateConditionalJumpInstruction(bb.Instructions.Last(), Instruction.ConditionType.AlwaysFalse, extraFake);
                    ExpandFakeLane(fake, mainRouteBB, false);
                    ExpandExtraFakeLane(extraFake, mainRouteBB, false);
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
                List<BasicBlock> basicblocks = funct.BasicBlocks.FindAll(x => x.Instructions.Last().statementType == ExchangeFormat.StatementTypeType.EnumValues.eConditionalJump &&
                                                                            x.Instructions.Last().RefVariables.Count() == 1);
                foreach (BasicBlock bb in basicblocks)
                {
                    InsertConditionals(bb);
                }
            }            
        }

        /// <summary>
        /// Expands the fake lane into the CFT and creates the conditional jumps for both Main Routa and Extra Fake code
        /// </summary>
        /// <param name="fake1">The first basic block of the Fake Lane</param>
        /// <param name="mainLaneBB">The basic block we should go to be back in the Main Route</param>
        /// <param name="atFakeCodeGeneration">Wheter we are or not at the fake code generation phase</param>
        private static void ExpandFakeLane(BasicBlock fake1, BasicBlock mainRouteBB, bool atFakeCodeGeneration)
        {
            //Fetching the Conditional Jump (AlwaysFalse) created in the previous basic block
            Instruction originalInstruction = fake1.getPredecessors.First().Instructions.Last();

            //Defining the constant and relational operator for the fake1 conditional jump 
            Instruction.RelationalOperationType relationalOperation;
            int rightValue = 0;
            if (originalInstruction.GetConstFromCondition() >= originalInstruction.GetVarFromCondition().fixedMax.Value)
            {
                rightValue = Randomizer.SingleNumber(Common.GlobalMinValue, originalInstruction.GetVarFromCondition().fixedMin.Value);
                relationalOperation = (Instruction.RelationalOperationType)
                                                        Randomizer.OneFromMany(Instruction.RelationalOperationType.Smaller,
                                                                            Instruction.RelationalOperationType.SmallerOrEquals);
            }
            else
            {
                rightValue = Randomizer.SingleNumber(originalInstruction.GetVarFromCondition().fixedMax.Value, Common.GlobalMaxValue);
                relationalOperation = (Instruction.RelationalOperationType)
                                                        Randomizer.OneFromMany(Instruction.RelationalOperationType.Greater,
                                                                            Instruction.RelationalOperationType.GreaterOrEquals);
            }

            //Creating the false lane for the conditional jump that goes back to the Main Route
            fake1.LinkToSuccessor(ExpandFakeLane(fake1, mainRouteBB), true);

            //Creating fake2 to hold the true lane for the conditional jump
            BasicBlock fake2 = new BasicBlock(fake1.parent);

            //Creating the fake1 conditional jump
            Variable var = originalInstruction.GetVarFromCondition();
            fake1.Instructions.Last().MakeConditionalJump(var, rightValue, relationalOperation, fake2);
            //Expanding the extra fake lane
            ExpandExtraFakeLane(fake2, mainRouteBB, atFakeCodeGeneration);            
        }

        /// <summary>
        /// Expands the fake lane into the CFT that goes back to the Main Route
        /// </summary>
        /// <param name="bb">The actual basic block that will hold the conditional jump</param>
        /// <param name="originaltarget">The basic block we should go to be back in the Main Route</param>
        private static BasicBlock ExpandFakeLane(BasicBlock bb, BasicBlock originaltarget)
        {
            // Creating a new basic block
            BasicBlock fake1 = new BasicBlock(bb.parent);
            fake1.Instructions.Add(new Instruction(fake1));

            // Creating the second fake block
            BasicBlock fake2 = new BasicBlock(bb.parent);
            fake2.Instructions.Add(new Instruction(fake2));

            // Starting the linking
            fake1.Instructions.Last().MakeUnconditionalJump(fake2);

            BasicBlock polyrequtarget = null;

            // Creating a clone of the original target in order to make the CFT more obfuscated
            if (originaltarget.Instructions.Last().statementType == ExchangeFormat.StatementTypeType.EnumValues.eUnconditionalJump)
            {
                polyrequtarget = new BasicBlock(originaltarget, originaltarget.getSuccessors);
                polyrequtarget.Instructions.ForEach(delegate(Instruction inst) { inst.polyRequired = true; });
            }
            else polyrequtarget = originaltarget;

            // And now setting the edges
            fake2.Instructions.Last().MakeUnconditionalJump(polyrequtarget);

            // And then converting its nop instruction into a ConditionalJump
            Randomizer.GenerateConditionalJumpInstruction(fake1.Instructions.Last(), Instruction.ConditionType.Random, originaltarget);

            //bb.LinkToSuccessor(fake1, true);
            //bb.parent.Validate();
            return fake1;

        }

        /// <summary>
        /// Expands the extra fake lane into the CFT and creates the conditional jumps for both Loop and Extra Fake code
        /// </summary>
        /// <param name="extraFake1">The first basic block of the Extra Fake Lane</param>
        /// <param name="mainLaneBB">The basic block we should go in case of no-loop</param>
        /// <param name="atFakeCodeGeneration">Wheter we are or not at the fake code generation phase</param>
        public static void ExpandExtraFakeLane(BasicBlock extraFake1, BasicBlock mainRouteBB, bool atFakeCodeGeneration)
        {
            //Creating extraFake2 to hold the next Loop condition
            BasicBlock extraFake2 = new BasicBlock(extraFake1.parent);
            extraFake2.dead = true;

            //Creating extraFake3 to hold the extra fake code in case of No-Loop
            BasicBlock extraFake3 = new BasicBlock(extraFake1.parent);
            extraFake3.Involve = BasicBlock.InvolveInFakeCodeGeneration.OriginalVariablesOnly;
            extraFake3.dead = true;

            //Creating the extraFake3 unconditional jump back to the Main Lane
            extraFake3.Instructions.Last().MakeUnconditionalJump(mainRouteBB);

            //Linking extraFake3 to extraFake2
            extraFake2.LinkToSuccessor(extraFake3);

            //Creating extraFake4 to hold the extra fake code in case of No-Loop
            BasicBlock extraFake4 = new BasicBlock(extraFake1.parent);
            extraFake4.Involve = BasicBlock.InvolveInFakeCodeGeneration.OriginalVariablesOnly;
            extraFake4.dead = true;

            //Creating the extraFake4 unconditional jump back to the Main Lane
            extraFake4.Instructions.Last().MakeUnconditionalJump(mainRouteBB);

            //Linking extraFake4 to extraFake1
            extraFake1.LinkToSuccessor(extraFake4);

            //Fetching the Conditional Jump (AlwaysFalse) created in the previous basic block
            Instruction originalInstruction = extraFake1.getPredecessors.First().Instructions.Last();

            //Defining the relational operator for the extraFake1 conditional jump
            Instruction.RelationalOperationType relationalOperationEF1 = (Instruction.RelationalOperationType)
                                                        Randomizer.OneFromMany(Instruction.RelationalOperationType.Smaller,
                                                                            Instruction.RelationalOperationType.SmallerOrEquals,
                                                                            Instruction.RelationalOperationType.Greater,
                                                                            Instruction.RelationalOperationType.GreaterOrEquals);

            //Defining the constant for the extraFake1 conditional jump
            int rightValueEF1 = 0;
            if (originalInstruction.GetConstFromCondition() >= originalInstruction.GetVarFromCondition().fixedMax.Value)
                rightValueEF1 = Randomizer.SingleNumber((int)originalInstruction.GetConstFromCondition(), Common.GlobalMaxValue);
            else
                rightValueEF1 = Randomizer.SingleNumber(Common.GlobalMinValue, (int)originalInstruction.GetConstFromCondition());

            //Creating the extraFake1 conditional jump
            Variable var = originalInstruction.GetVarFromCondition();
            extraFake1.Instructions.Last().MakeConditionalJump(var, rightValueEF1, relationalOperationEF1, extraFake2);

            //Defining the range for the extraFake2 conditional jump constant
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

            //Defining the constant for the extraFake2 conditional jump
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

            //Selecting the target for extraFake2 in order to create a loop
            List<BasicBlock> reacheableBasicBlocks = DataAnalysis.GetReachableBasicBlocks(extraFake2, Common.Direction.Up);
            //In case we are at fake code generation, we should choose reachable basic blocks in a loop body
            //in order to make the CFG irreducible
            if (atFakeCodeGeneration)
            {
                reacheableBasicBlocks = reacheableBasicBlocks.FindAll(x => DataAnalysis.isLoopBody.Keys.Contains(x) 
                    && DataAnalysis.isLoopBody[x] == true);
            }
            BasicBlock loopTarget = null;
            //In case we don't have basic blocks that fulfill our requirements, we get a random one
            if (reacheableBasicBlocks.Count == 0)
                loopTarget = Randomizer.JumpableBasicBlock(extraFake2.parent);
            else
                loopTarget = (BasicBlock) Randomizer.OneFromMany(reacheableBasicBlocks.ToArray());

            //Creating the extraFake2 conditional jump to the loop target
            extraFake2.Instructions.Last().MakeConditionalJump(var, rightValueEF2, relationalOperationEF2, loopTarget);
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
                            bblist[i].LinkToSuccessor(Randomizer.GeneratePolyRequJumpTarget(falselist), true);
                        bblist[i].Instructions.Last().MakeConditionalJump(var, condlist[i].value, condlist[i].relop, Randomizer.GeneratePolyRequJumpTarget(truelist));
                        break;
                    case Cond.BlockJumpType.False:
                        if (i != condlist.Count() - 1)
                            bblist[i].LinkToSuccessor(bblist[i + 1], true);
                        else
                            bblist[i].LinkToSuccessor(Randomizer.GeneratePolyRequJumpTarget(falselist), true);
                        bblist[i].Instructions.Last().MakeConditionalJump(var, condlist[i].value, condlist[i].relop, Randomizer.GeneratePolyRequJumpTarget(falselist));
                        break;
                    case Cond.BlockJumpType.Ambigious:
                        BasicBlock ambhelper = new BasicBlock(bb.parent);
                        bblist[i].LinkToSuccessor(ambhelper, true);
                        ambhelper.Instructions.Add(new Instruction(ambhelper));
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
                            ambhelper.Instructions.Last().MakeUnconditionalJump(Randomizer.GeneratePolyRequJumpTarget(falselist));
                        else
                            ambhelper.Instructions.Last().MakeUnconditionalJump(Randomizer.GeneratePolyRequJumpTarget(truelist));
                        bblist[i].Instructions.Last().MakeConditionalJump(var, condlist[i].value, condlist[i].relop, bblist[i + 1]);
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

                if (i > 0) i++;
                else if (i < -1) i--;
                i *= -1;
            }
            returnlist.Shuffle<Cond>();
            RepositionLasts(returnlist);
            return returnlist;
        }
    }
}

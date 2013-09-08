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
        /// <param name="rtn">Thse routine which will be meshed up</param>
        public static void MeshUnconditionals(Routine rtn)
        {
            /// Meshing of the unconditional jumps
            foreach (Function funct in rtn.Functions)
            {
                List<BasicBlock> basicblocks = funct.BasicBlocks.FindAll(x => x.Instructions.Last().statementType == ExchangeFormat.StatementTypeType.EnumValues.eUnconditionalJump);
                foreach (BasicBlock bb in basicblocks)
                {
                    bb.LinkToSuccessor(InsertFakeLane(bb), true);
                    Randomizer.GenerateConditionalJumpInstruction(bb.Instructions.Last(), Instruction.ConditionType.AlwaysFalse, InsertDeadLane(bb));
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
                List<BasicBlock> basicblocks = funct.BasicBlocks.FindAll(x => x.Instructions.Last().statementType == ExchangeFormat.StatementTypeType.EnumValues.eConditionalJump);
                foreach (BasicBlock bb in basicblocks)
                {
                    InsertConditionals(bb);
                }
            }            
        }

        /// <summary>
        /// Inserts the fake lane into the CFT
        /// Still in test phase, now it only has a not so smart condition in the conditional jump
        /// </summary>
        /// <param name="bb">The actual basic block with the unconditional jump</param>
        private static BasicBlock InsertFakeLane(BasicBlock bb)
        {
            // Saving the original target of the jump
            BasicBlock originaltarget = bb.getSuccessors.First();

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

            bb.LinkToSuccessor(fake1, true);
            bb.parent.Validate();
            return fake1;
            
        }

        /// <summary>
        /// Inserts the dead lane into the CFT and also changing the original unconditional jump into a conditional
        /// In thest phase -> condition not always false
        /// </summary>
        /// <param name="bb">The actual basic block with the unconditional jump</param>
        private static BasicBlock InsertDeadLane(BasicBlock bb)
        {

            // First, creating a basic block pointing to a random block of the function
            BasicBlock dead1 = new BasicBlock(bb.parent);

            // And making it dead
            dead1.dead = true;

            // Now including another one, with the basicblock splitter
            BasicBlock dead2 = new BasicBlock(bb.parent);

            // And making it dead too
            dead2.dead = true;

            dead1.LinkToSuccessor(dead2);

            // Now including the third one, with the basicblock splitter
            BasicBlock dead3 = new BasicBlock(bb.parent);

            // And making it dead too
            dead3.dead = true;

            // Now creating the conditional jump
            Randomizer.GenerateConditionalJumpInstruction(dead1.Instructions.Last(), Instruction.ConditionType.Random, dead3);

            // Random linking
            dead2.Instructions.Add(new Instruction( dead2 ));
            dead2.Instructions.Last().MakeUnconditionalJump(Randomizer.JumpableBasicBlock(bb.parent));
            dead3.Instructions.Add(new Instruction( dead3 ));
            dead3.Instructions.Last().MakeUnconditionalJump(Randomizer.JumpableBasicBlock(bb.parent));

            return dead1;
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
            Variable var = null;
            Instruction.RelationalOperationType relop = 0;
            int C = 0;
            BasicBlock truesucc = null;
            BasicBlock falsesucc = null;
            Parser.ConditionalJump(bb.Instructions.Last(), out var, out C, out relop, out truesucc, out falsesucc);
            List<Cond> condlist = GenetateCondList(C, relop);
            GenerateBlocks(bb, var, truesucc, falsesucc, condlist);
        }

        /// <summary>
        /// This function generates the BasicBlocks into the function, based on the condition list
        /// </summary>
        /// <param name="bb">The actual basicblock with the conditional jump at the end</param>
        /// <param name="truelane">The true successor</param>
        /// <param name="falselane">The false successor</param>
        /// <param name="condlist">The condition list</param>
        /// <returns>The list of the generated BasicBlocks</returns>
        private static void GenerateBlocks(BasicBlock bb, Variable var, BasicBlock truelane, BasicBlock falselane, List<Cond> condlist)
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
                        ambhelper.Instructions.Last().MakeUnconditionalJump(Randomizer.GeneratePolyRequJumpTarget(falselist));
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
            int num = Common.ConditionalJumpReplacementFactor;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Internal;
using ExchangeFormat;
using System.Text.RegularExpressions;

namespace Obfuscator
{
    public static class FakeCode
    {
        /*
         * At a certain probability we generate a Conditional Jump, which condition
         * depends on a parameter value, and happens to be always false.
         * We use these jumps to make the control flow irreducible.
         * However we must not choose this if we have only one basic block
         * apart from the "fake exit block". So we must have at least 3 BBs.
         * 
         * At the remaining probability we generate instructions that are
         * using the dead variables present at the point of the actual nop.
         * However we must not choose this if we have no dead variables
         * available for using as a left value present.
         */

        /// <summary>
        /// Describes the probability of generating a conditional jump in percents.
        /// </summary>
        private static int prob_of_cond_jump = 30;

        /// <summary>
        /// Returns a list of all nops present in the function.
        /// </summary>
        /// <param name="func">The actual function.</param>
        /// <returns>A list of nops.</returns>
        private static List<Instruction> GetAllNops(Function func)
        {
            List<Instruction> nops = new List<Instruction>();
            foreach (BasicBlock bb in func.BasicBlocks)
            {
                foreach (Instruction ins in bb.Instructions)
                {
                    if (ins.statementType == StatementTypeType.EnumValues.eNoOperation)
                        nops.Add(ins);
                }
            }
            return nops;
        }

        /// <summary>
        /// Function to change the nop's in the function to actual fake instructons.
        /// </summary>
        /// <param name="func">The function to work on.</param>
        public static void GenerateFakeInstructions(Function func)
        {
            /* We have to go through all the nop's in the function. */
            List<Instruction> nops = GetAllNops(func);
            
            foreach (Instruction ins in nops)
            { 
                if (( DataAnalysis.isMainRoute(ins.parent) && Randomizer.DeadVariable(ins, Variable.State.Free, Variable.State.Not_Initialized) == null ) ||
                    ( !DataAnalysis.isMainRoute(ins.parent) && Randomizer.DeadVariable(ins, Variable.State.Free) == null )                                   )
                    continue;

                /* If a conditional jump cannot be made here, or we didn't choose it, we generate a fake instruction. */
                if (func.BasicBlocks.Count < 2 || Randomizer.SingleNumber(0, 99) >= prob_of_cond_jump)
                {
                    _GenerateFakeInstruction(ins);
                       
                    /* If the instruction is made, then we have to refresh the states of the dead variables. */
                    if (ins.statementType != StatementTypeType.EnumValues.eNoOperation)
                        ins.RefreshNext();
                }   
            }

            /* Now we check for forbidden state collisions at every basic blocks' beginning. */
            foreach (BasicBlock bb in func.BasicBlocks)
            {
                if (DataAnalysis.ForbiddenStateCollision(bb.Instructions.First()))
                    throw new ObfuscatorException("Forbidden state collision: FILLED meets NOT_INITIALIZED.");
            }
        }

        /// <summary>
        /// Function to change the remaining nop's in the function to conditional jumps.
        /// </summary>
        /// <param name="func">The function to work on.</param>
        public static void GenerateConditionalJumps(Function func)
        {
            /* We have to go through all the nop's in the function. */
            List<Instruction> nops = GetAllNops(func);

            foreach (Instruction ins in nops)
            {
                if (func.BasicBlocks.Count < 2)
                    continue;

                _GenerateConditionalJump(ins);
            }
        }

        /// <summary>
        /// Function to make a Conditional Jump out of a Nop.
        /// </summary>
        /// <param name="ins">The nop we want to work on.</param>
        private static void _GenerateConditionalJump(Instruction ins)
        {
            /* 
             * Before doing anything, we have to split the basic block holding this
             * instruction, so we can make a conditional jump at the end of the
             * new basic block, unless it is already the last instruction.
             */
            if ( !ins.Equals(ins.parent.Instructions.Last()) )
                ins.parent.SplitAfterInstruction(ins);

            /* We get a jump target. */
            BasicBlock jumptarget = Randomizer.JumpableBasicBlock(ins.parent.parent);
            
            /* We make a random conditional jump here, which has to be always false. */
            Randomizer.GenerateConditionalJumpInstruction(ins, Instruction.ConditionType.AlwaysFalse, jumptarget);
        }

        /// <summary>
        /// Function to make an actual fake instruction out of a Nop.
        /// </summary>
        /// <param name="ins">The nop we want to work on.</param>
        private static void _GenerateFakeInstruction(Instruction ins)
        {
            /* First we get a left value. */
            Variable leftvalue;

            if (DataAnalysis.isMainRoute(ins.parent))
                leftvalue = Randomizer.DeadVariable(ins, Variable.State.Free, Variable.State.Not_Initialized);

            /* If we aren't in the main route, then we cannot use NOT_INITIALIZED ones as left value. */
            else
                leftvalue = Randomizer.DeadVariable(ins, Variable.State.Free);

            /* Then we check how many variables can we use as right value. */
            List<Variable> rightvalues = GetRandomRightValues(ins, 2);

            /* We random generate a statement type which we may not use according to the available right values. */
            StatementTypeType.EnumValues statementType =
                (StatementTypeType.EnumValues) Randomizer.OneFromMany ( StatementTypeType.EnumValues.eCopy              ,
                                                                        StatementTypeType.EnumValues.eUnaryAssignment   ,
                                                                        StatementTypeType.EnumValues.eFullAssignment    );

            /*
             * The number of the available right values determines which kind of instructions
             * can we generate.
             * 
             *  - If we don't have any, then we must make a Copy with a constant.
             * 
             *  - If we have one, and it's the same as the left value, then we have to make
             *    a Unary Assignment.
             *    
             *  - If we have one, but it's different from the left value, then we can make
             *    a Copy (1), a Unary Assignment (2) and a Full Assignment with a constant (3).
             *    
             *  - If we have two, we can make all these and a Full Assignment with two
             *    variables.
             *    
             * Another important thing is that we cannot fill the variables with a value
             * without using themselves (like t = t + 1) while inside a loop.
             */
            switch (rightvalues.Count)
            {
                case 0:
                    /* 
                     * If we have no available right values, and we are in a loop, then we can't do anything.
                     * ( maybe t = -t, but that isn't quite reasonable... )
                     */
                    if (DataAnalysis.isLoopBody(ins.parent))
                        return;

                    /* We don't have any right values, so we have to copy a constant value. */
                    else
                        ins.MakeCopy(leftvalue, null, Randomizer.SingleNumber(Common.GlobalMinNumber, Common.GlobalMaxNumber));
                    break;

                case 1:
                    /* We choose Full Assignment, or rightvalue is the same as leftvalue, or we are in a loop body. */
                    if (DataAnalysis.isLoopBody(ins.parent) || leftvalue.Equals(rightvalues[0]) 
                                                            || statementType == StatementTypeType.EnumValues.eFullAssignment)
                    {
                        /* Here we random generate an operator. */
                        Instruction.ArithmeticOperationType op = 
                            (Instruction.ArithmeticOperationType) Randomizer.OneFromMany(Instruction.ArithmeticOperationType.Addition        ,
                                                                                         Instruction.ArithmeticOperationType.Division        ,
                                                                                         Instruction.ArithmeticOperationType.Multiplication  ,
                                                                                         Instruction.ArithmeticOperationType.Subtraction     );
                        
                        if (op == Instruction.ArithmeticOperationType.Addition || op == Instruction.ArithmeticOperationType.Subtraction)
                            ins.MakeFullAssignment(leftvalue, leftvalue, null, Randomizer.SingleNumber(Common.GlobalMinNumber, Common.GlobalMaxNumber), op);

                        else
                        {
                            /* 
                             * Here we generate an integer number which is the power of 2, and is
                             * within the boundaries of GlobalMinNumber and GlobalMaxNumber.
                             */
                            int min = Convert.ToInt32(Math.Ceiling(Math.Log(Common.GlobalMinNumber, 2)));
                            int max = Convert.ToInt32(Math.Floor(Math.Log(Common.GlobalMaxNumber, 2)));
                            int pow = Randomizer.SingleNumber(min, max);
                            int num = Convert.ToInt32(Math.Pow(2, pow));

                            ins.MakeFullAssignment(leftvalue, leftvalue, null, num, op);
                        }
                    }

                    /* If we choose Unary Assignment. */
                    else if (statementType == StatementTypeType.EnumValues.eUnaryAssignment)
                        ins.MakeUnaryAssignment(leftvalue, rightvalues.First(), Instruction.UnaryOperationType.ArithmeticNegation);

                    /* If we choose Copy. */
                    else
                        ins.MakeCopy(leftvalue, rightvalues.First(), null);
                    break;

                case 2:
                    /* We make sure that the first right value isn't the same as the left value. */
                    if (leftvalue.Equals(rightvalues.First()))
                    {
                        Variable tmp = rightvalues[0];
                        rightvalues[0] = rightvalues[1];
                        rightvalues[1] = tmp;
                    }

                    /* 
                     * If we are in a loop body, then we can only make a full assignment like that:
                     * t1 = t2 op t1
                     */
                    if (DataAnalysis.isLoopBody(ins.parent) || statementType == StatementTypeType.EnumValues.eFullAssignment)
                    {
                        /* Here we random generate an operator: + or - */
                        /* TODO: (efficient) * and / */
                        Instruction.ArithmeticOperationType op =
                            (Instruction.ArithmeticOperationType)Randomizer.OneFromMany(Instruction.ArithmeticOperationType.Addition,
                                                                                        Instruction.ArithmeticOperationType.Subtraction);

                        if (DataAnalysis.isLoopBody(ins.parent))
                            ins.MakeFullAssignment(leftvalue, rightvalues[0], leftvalue, null, op);

                        else
                            ins.MakeFullAssignment(leftvalue, rightvalues[0], rightvalues[1], null, op);
                    }
                    
                    /* If we choose Copy. */
                    else if (statementType == StatementTypeType.EnumValues.eCopy)
                        ins.MakeCopy(leftvalue, rightvalues.First(), null);

                    /* If we choose Unary Assignment. */
                    else
                        ins.MakeUnaryAssignment(leftvalue, rightvalues.First(), Instruction.UnaryOperationType.ArithmeticNegation);
                    break;
            }
        }

        /// <summary>
        /// Returns a list of variables from the instruction's DeadVariables list with FREE or FILLED state.
        /// </summary>
        /// <param name="ins">The actual instruction.</param>
        /// <param name="amount">Shows how many right values do we need.</param>
        /// <returns>A list of proper variables which may hold less variables than needed.</returns>
        private static List<Variable> GetRandomRightValues(Instruction ins, int amount)
        {
            /* First we gather the variables with the proper state. */
            List<Variable> proper_vars = new List<Variable>();
            proper_vars = ins.DeadVariables.Keys.ToList().FindAll(x => ins.DeadVariables[x] == Variable.State.Free || ins.DeadVariables[x] == Variable.State.Filled);

            /* If we have less proper variables than needed: we return as many as we have. */
            if (proper_vars.Count <= amount)
                return proper_vars;
            /* If we have more, then we pick randomly. */
            else
                return Randomizer.UniqueSelect<Variable>(proper_vars, amount).ToList();
        }


        /// <summary>
        /// Fills each basic block of a function by  NoOperation instructions (according to FPO)
        /// </summary>
        /// <param name="func_orig">The function containing basic blocks</param>
        public static void GenerateNoOperations(Function func_orig)
        {
            foreach (BasicBlock bb in func_orig.BasicBlocks)
            {
                /* If this is the "fake exit block", then we shouldn't fill it with nops. */
                if (bb.getSuccessors.Count == 0)
                    continue;

                List<Instruction> new_list = new List<Instruction>();
                foreach (Instruction inst in bb.Instructions)
                {
                    List<Instruction> nops_and_original = new List<Instruction>();
                    // If the instruction is fake, do not generate additional NOPs
                    if (inst.isFake)
                        nops_and_original.Add(inst);
                    // If the instruction is original, generate FPO number of NOPs
                    else
                    {
                        for (int i = 0; i < Common.FPO; i++)
                            nops_and_original.Add(new Instruction(bb));

                        if (inst.statementType == StatementTypeType.EnumValues.eConditionalJump ||
                            inst.statementType == StatementTypeType.EnumValues.eUnconditionalJump ||
                            (inst.statementType == StatementTypeType.EnumValues.eProcedural && Regex.IsMatch(inst.TACtext, @"^return ", RegexOptions.None)))
                            nops_and_original.Add(inst);  // Add original to the tail
                        else
                            nops_and_original.Insert(Randomizer.SingleNumber(0, Common.FPO), inst);    // Mesh original between the NOPs
                    }
                    new_list.AddRange(nops_and_original);
                }
                bb.Instructions = new_list;
                if (bb.Instructions.Count < Common.FakePadding)
                {
                    int fakes = Math.Abs(Randomizer.SingleNumber(Common.FakePadding - Common.FakePaddingVariance, Common.FakePadding + Common.FakePaddingVariance) - bb.Instructions.Count);
                    for (int i = 0; i < fakes; i++)
                        bb.Instructions.Insert(0, new Instruction(bb));
                }
            }
        }
    }
}

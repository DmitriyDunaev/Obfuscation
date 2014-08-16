using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Objects;
using System.Text.RegularExpressions;
using Services;

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
         * At the remaining nops we generate instructions that are
         * using the dead variables present at the point of the actual nop.
         */
                
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
                    if (ins.statementType == Objects.Common.StatementType.NoOperation)
                        nops.Add(ins);
                }
            }
            return nops;
        }

        /// <summary>
        /// Returns a proper left value for the instruction, or null if no such exists.
        /// </summary>
        /// <param name="ins">The actual instruction.</param>
        /// <returns>A proper variable, or null.</returns>
        private static Variable GetLeftValueForInstruction(Instruction ins)
        {
            Variable left = null;
            switch (ins.parent.Involve)
            {
                case BasicBlock.InvolveInFakeCodeGeneration.FakeVariablesOnly:
                    left = Randomizer.DeadVariable(ins, Variable.State.Free, Variable.State.Not_Initialized, Variable.State.Filled);
                    break;

                case BasicBlock.InvolveInFakeCodeGeneration.OriginalVariablesOnly:
                    List<Variable> originalVars = ins.parent.parent.LocalVariables.FindAll(x => x.fake == false && !ins.DeadVariables.Keys.Contains(x));
                    if (originalVars.Count > 0)
                        left = (Variable)Randomizer.OneFromMany(originalVars.ToArray());
                    break;
                case BasicBlock.InvolveInFakeCodeGeneration.Both:
                    List<Variable> fakeVars = ins.DeadVariables.Keys.ToList().FindAll(x => ins.DeadVariables[x] != Variable.State.Used);
                    List<Variable> origVars = ins.parent.parent.LocalVariables.FindAll(x => x.fake == false && !ins.DeadVariables.Keys.Contains(x) && x.pointer == false);
                    if (fakeVars.Count > 0 && origVars.Count > 0)
                        left = (Variable)Randomizer.OneValueFromManySetsWithProbability(new int[2] { 40, 60 },
                            fakeVars,
                            origVars);
                    else if (fakeVars.Count > 0 && origVars.Count == 0)
                        left = (Variable)Randomizer.OneFromMany(fakeVars.ToArray());
                    else if (fakeVars.Count == 0 && origVars.Count > 0)
                        left = (Variable)Randomizer.OneFromMany(origVars.ToArray());
                    break;
            }

            if (left == null || DataAnalysis.isMainRoute[ins.parent])
                return left;

                    /* If we aren't in the main route, then we cannot use NOT_INITIALIZED ones as left value. */
            else
            {
                if (ins.DeadVariables.Keys.Contains(left) && ins.DeadVariables[left] == Variable.State.Not_Initialized)
                    return Randomizer.DeadVariable(ins, Variable.State.Free, Variable.State.Filled);
                else
                    return left;
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
            proper_vars = ins.DeadVariables.Keys.ToList().FindAll(x => ins.DeadVariables[x] != Variable.State.Not_Initialized);
            proper_vars.AddRange(ins.parent.parent.LocalVariables.FindAll(x => x.kind == Variable.Kind.Input));
            proper_vars = proper_vars.Distinct().ToList();
                    
            /* If we have less proper variables than needed: we return as many as we have. */
            if (proper_vars.Count <= amount)
                return proper_vars;
            /* If we have more, then we pick randomly. */
            else
                return Randomizer.UniqueSelect<Variable>(proper_vars, amount).ToList();
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
                if (GetLeftValueForInstruction(ins) == null)
                    continue;

                _GenerateFakeInstruction(ins);

                /* If the instruction is made, then we have to refresh the states of the dead variables. */
                if (ins.statementType != Objects.Common.StatementType.NoOperation)
                    RefreshNext(ins);                
            }

            /* Now we check for forbidden state collisions at every basic blocks' beginning. */
            foreach (BasicBlock bb in func.BasicBlocks)
            {
                if (DataAnalysis.ForbiddenStateCollision(bb.Instructions.First()))
                    throw new ObfuscatorException("Forbidden state collision: FILLED meets NOT_INITIALIZED.");
            }

            //Checking whether we have "nops" left
            if (GetAllNops(func).Count > 0)
                throw new ObfuscatorException("Lack of available dead variables.");
        }

        /// <summary>
        /// Function to make an actual fake instruction out of a Nop.
        /// </summary>
        /// <param name="ins">The nop we want to work on.</param>
        private static void _GenerateFakeInstruction(Instruction ins)
        {
            /* First we get a left value. */
            Variable leftvalue = GetLeftValueForInstruction(ins);

            /* Then we check how many variables can we use as right value. */
            List<Variable> rightvalues = GetRandomRightValues(ins, 2);

            /* We random generate a statement type which we may not use according to the available right values. */
            Objects.Common.StatementType statementType =
                (Objects.Common.StatementType)Randomizer.OneFromManyWithProbability(
                                                                        new int[3] { 25, 15, 60 }                       ,
                                                                        Objects.Common.StatementType.Copy,
                                                                        Objects.Common.StatementType.UnaryAssignment,
                                                                        Objects.Common.StatementType.FullAssignment);

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
                    /* If we have no available right values, and we are in a loop, then we can't do anything. */
                    if (DataAnalysis.isLoopBody[ins.parent])
                        return;

                    /* We don't have any right values, so we have to copy a constant value. */
                    else
                        MakeInstruction.Copy(ins, leftvalue, null, Randomizer.SingleNumber(Common.GlobalMinValue, Common.GlobalMaxValue));
                    break;

                case 1:
                    /* We choose Full Assignment, or rightvalue is the same as leftvalue, or we are in a loop body. */
                    if (DataAnalysis.isLoopBody[ins.parent] || leftvalue.Equals(rightvalues[0])
                                                            || statementType == Objects.Common.StatementType.FullAssignment
                                                            || (ins.DeadVariables.Keys.Contains(leftvalue) 
                                                            && ins.DeadVariables[leftvalue] == Variable.State.Filled))
                    {
                        /* Here we random generate an operator. */
                        Instruction.ArithmeticOperationType op = 
                            (Instruction.ArithmeticOperationType) Randomizer.OneFromMany(Instruction.ArithmeticOperationType.Addition        ,
                                                                                         Instruction.ArithmeticOperationType.Division        ,
                                                                                         Instruction.ArithmeticOperationType.Multiplication  ,
                                                                                         Instruction.ArithmeticOperationType.Subtraction     );

                        int num;
                        
                        if (op == Instruction.ArithmeticOperationType.Addition || op == Instruction.ArithmeticOperationType.Subtraction)
                            num = Randomizer.SingleNumber(Common.GlobalMinValue, Common.GlobalMaxValue);

                        else
                        {
                            /* 
                             * Here we generate an integer number which is the power of 2, and is
                             * within the boundaries of GlobalMinNumber and GlobalMaxNumber.
                             */
                            int min = Convert.ToInt32(Math.Ceiling(Math.Log(Common.GlobalMinValue, 2)));
                            int max = Convert.ToInt32(Math.Floor(Math.Log(Common.GlobalMaxValue, 2)));
                            int pow = Randomizer.SingleNumber(min, max);
                            num = Convert.ToInt32(Math.Pow(2, pow));
                        }

                        MakeInstruction.FullAssignment(ins, leftvalue, rightvalues.First(), null, num, op);
                    }

                    /* If we choose Unary Assignment. */
                    else if (statementType == Objects.Common.StatementType.UnaryAssignment)
                        MakeInstruction.UnaryAssignment(ins, leftvalue, rightvalues.First(), Instruction.UnaryOperationType.ArithmeticNegation);

                    /* If we choose Copy. */
                    else
                        MakeInstruction.Copy(ins, leftvalue, rightvalues.First(), null);
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
                    if (DataAnalysis.isLoopBody[ins.parent] || statementType == Objects.Common.StatementType.FullAssignment
                        || (ins.DeadVariables.Keys.Contains(leftvalue) 
                        && ins.DeadVariables[leftvalue] == Variable.State.Filled))
                    {
                        /* Here we random generate an operator: + or - */
                        /* TODO: (efficient) * and / */
                        Instruction.ArithmeticOperationType op =
                            (Instruction.ArithmeticOperationType)Randomizer.OneFromMany(Instruction.ArithmeticOperationType.Addition,
                                                                                        Instruction.ArithmeticOperationType.Subtraction);

                        if (DataAnalysis.isLoopBody[ins.parent]
                            || (ins.DeadVariables.Keys.Contains(leftvalue) 
                            && ins.DeadVariables[leftvalue] == Variable.State.Filled))
                            MakeInstruction.FullAssignment(ins, leftvalue, rightvalues[0], leftvalue, null, op);

                        else
                            MakeInstruction.FullAssignment(ins, leftvalue, rightvalues[0], rightvalues[1], null, op);
                    }
                    
                    /* If we choose Copy. */
                    else if (statementType == Objects.Common.StatementType.Copy)
                        MakeInstruction.Copy(ins, leftvalue, rightvalues.First(), null);

                    /* If we choose Unary Assignment. */
                    else
                        MakeInstruction.UnaryAssignment(ins, leftvalue, rightvalues.First(), Instruction.UnaryOperationType.ArithmeticNegation);
                    break;
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

                int i = Randomizer.SingleNumber(0, 100);
                if (i <= Common.ConditionalJumpProbability)
                {
                    _GenerateConditionalJump(ins);
                }
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
            if (!ins.Equals(ins.parent.Instructions.Last()))
                ins.parent.SplitAfterInstruction(ins);

            
            // We create a jump target.
            BasicBlock jumptarget = new BasicBlock(ins.parent.parent);

            // We make a random conditional jump here, which has to be always false.
            MakeInstruction.RandomConditionalJump(ins, Instruction.ConditionType.AlwaysFalse, jumptarget);

            //We expand the Fake Route as we did during meshing
            Meshing.ExpandFakeRoute(jumptarget, ins.parent.getSuccessors.Last(), true);
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

                        // Add original to TAIL
                        if (inst.statementType == Objects.Common.StatementType.ConditionalJump ||
                            inst.statementType == Objects.Common.StatementType.UnconditionalJump ||
                            (inst.statementType == Objects.Common.StatementType.Procedural &&
                                    Regex.IsMatch(inst.TACtext, @"^return ", RegexOptions.None) ||
                                    Regex.IsMatch(inst.TACtext, @"^call ", RegexOptions.None)
                                    ))
                            nops_and_original.Add(inst);
                        // Add original to HEAD
                        else if (inst.statementType == Objects.Common.StatementType.Procedural &&
                            Regex.IsMatch(inst.TACtext, @"^retrieve ", RegexOptions.None))
                            nops_and_original.Insert(0, inst);
                        // Mesh original between NOPs
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


        /*
 * When we modify a fake instruction, we change the states in it, so we
 * have to update the dead variables in the following instructions.
 */
        /// <summary>
        /// Refreshes the state of all following instructions' dead variables.
        /// </summary>
        public static void RefreshNext(Instruction ins)
        {
            /*
             * For every used dead variable in the instruction we should first determine
             * its new state, then we could push it's (changed) state through all the
             * following instructions, so it will get the appropriate state everywhere.
             */
            foreach (Variable var in ins.RefVariables)
            {
                if (ins.DeadVariables.ContainsKey(var))
                {
                    List<Variable.State> states = ins.GetChangedStates(var);

                    if (states.Count() == 0)
                    {
                        /* Nothing has changed. */
                        continue;
                    }

                    /* First we set the state of that used dead variable. */
                    ins.DeadVariables[var] = states.First();

                    /* Then we tell its (changed) state to the following instructions. */
                    foreach (Instruction inst in ins.GetFollowingInstructions())
                        RefreshNext(inst, var, ins.DeadVariables[var]);
                }
                else if (ins.DeadPointers.ContainsKey(var))
                {
                    List<Variable.State> states = ins.GetChangedStates(var);

                    /* First we set the state of that used dead pointer. */
                    ins.DeadPointers[var].State = states.First();

                    /* Then we tell its (changed) state to the following instructions. */
                    foreach (Instruction inst in ins.GetFollowingInstructions())
                        RefreshNext(inst, var, ins.DeadPointers[var].State);

                    /* 
                     * We set the state of the dead variable pointed by the dead pointer.
                     * 
                     * WARNING FOR FUTURE: (1)
                     */
                    if (states.Count() > 1)
                        ins.DeadVariables[ins.DeadPointers[var].PointsTo] = states[1];

                    /* Then we tell its (changed) state to the following instructions. */
                    foreach (Instruction inst in ins.GetFollowingInstructions())
                        RefreshNext(inst, ins.DeadPointers[var].PointsTo, ins.DeadVariables[ins.DeadPointers[var].PointsTo]);
                }
            }
        }


        /*
         * This function is called when we encounter a change in a dead variable's state.
         * So we want to push this change through all the instructions, and deal with
         * this variable only.
         * 
         * However we can't just overwrite the statement without any check...
         */
        private static void RefreshNext(Instruction ins, Variable var, Variable.State state)
        {
            /*
             * If this instruction uses this variable as well, or this instruction
             * uses a pointer that points to this variable then its state must not
             * be changed, because it's perfect as it is right now.
             */
            if (ins.RefVariables.Contains(var))
            {
                /* This instruction uses this variable. */
                return;
            }

            /*
             * We have to do anything only if the variable is in this instruction is
             * dead, and it's state differs from the new state.
             */
            if (ins.DeadVariables.ContainsKey(var)      // This variable is dead.
                && ins.DeadVariables[var] != state)     // The state differs.
            {
                foreach (Variable v in ins.RefVariables)
                {
                    if (ins.DeadPointers.ContainsKey(v) && ins.DeadPointers[v].PointsTo.Equals(var))
                    {
                        /* The instruction uses a pointer that points to this variable. */
                        return;
                    }
                }

                /*
                 * Now we are at the point, that maybe we have to change the state.
                 * Although it's not sure, because if the instruction has more than one predecessors,
                 * then the actual state might not change anything.
                 * For example: NOT_INITIALIZED meets FREE, or FREE meets FILLED.
                 * 
                 * NOTE: we don't know right now what to do if NOT_INIT meets FILLED...
                 */
                state = DataAnalysis.CheckPrecedingStates(ins, var, state);

                ins.DeadVariables[var] = state;
                foreach (Instruction inst in ins.GetFollowingInstructions())
                    RefreshNext(inst, var, state);
            }

            /*
             * Another case for doing something when we pass a dead pointer with changed state.
             * In this case we do not have to look for variables pointing to that one.
             * 
             * WARNING FOR FUTURE: (1)
             */
            if (ins.DeadPointers.ContainsKey(var)           // This is a dead pointer.
                && ins.DeadPointers[var].State != state)    // The states differ.
            {
                /* Just like in the previous case, we have to check all the preceding states for collisions. */
                state = DataAnalysis.CheckPrecedingStates(ins, var, state);

                ins.DeadPointers[var].State = state;
                foreach (Instruction inst in ins.GetFollowingInstructions())
                    RefreshNext(inst, var, state);
            }
        }





    }
}

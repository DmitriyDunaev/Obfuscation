using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Internal;
using ExchangeFormat;

namespace Obfuscator
{
    public static class FakeCode
    {
        /// <summary>
        /// Describes the probability of generating a conditional jump in percents.
        /// </summary>
        private static int prob_of_cond_jump = 30;
        private static int FPO = 1;
        private static int fake_padding = 0;
        private static int fake_padding_variability = 0;

        /// <summary>
        /// Function to change the nop's in the function to actual fake code.
        /// </summary>
        /// <param name="func">The function to work on.</param>
        public static void Generate (Function func)
        {
            /* We have to go through all the nop's in the function. */
            //************* Why not to use FOREACH?
            for (int j = 0; j < func.BasicBlocks.Count; j++)
            {
                BasicBlock bb = func.BasicBlocks[j];
                for (int i = 0; i < bb.Instructions.Count; i++)
                {
                    Instruction ins = bb.Instructions[i];
                    if (ins.statementType == StatementTypeType.EnumValues.eNoOperation)
                    {
                        /*
                         * Now we found a nop, so we have to decide what to do with it.
                         * 
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
                        if (/*func.BasicBlocks.Count < 3 && */GetRandomLeftValue(ins, DataAnalysis.isMainRoute(ins.parent)) == null)
                            continue;
                        //    throw new ObfuscatorException("We cannot really do anything with this instruction right now, sorry.");

                        //if (Randomizer.GetSingleNumber(0, 99) < prob_of_cond_jump)
                        //{
                        //    if (func.BasicBlocks.Count < 3)
                        //    {
                        //        /* Though randomizer said we should do this, we unfortunately cannot. */
                        //        GenerateFakeInstruction(ins);
                        //    }
                        //    /* This is where we generate a conditional jump.*/
                        //    GenerateConditionalJump(ins);
                        //}
                        //else
                        //{
                        //    if (GetRandomLeftValue(ins) == null)
                        //    {
                        //        /* Though randomizer said we should do this, we unfortunately cannot. */
                        //        GenerateConditionalJump(ins);
                        //    }
                            /* This is where we generate instructions with dead variables. */
                            GenerateFakeInstruction(ins);
                        //}

                        /* The instruction is made, so we have to refresh the states of the dead variables. */
                        ins.RefreshNext();
                    }
                }
            }
        }

        /// <summary>
        /// Function to make a Conditional Jump out of a Nop.
        /// </summary>
        /// <param name="ins">The nop we want to work on.</param>
        private static void GenerateConditionalJump(Instruction ins)
        {
            /* 
             * Before doing anything, we have to split the basic block holding this
             * instruction, so we can make a conditional jump at the and of the
             * new basic block.
             */
            ins.parent.SplitAfterInstruction(ins);

            BasicBlock jumptarget;
            /*
             * First we want to find a basic block that is inside a loop,
             * so that we can make a jump on it, making the control flow
             * irreducible this way.
             */
            List<BasicBlock> bodies = new List<BasicBlock>();
            foreach (BasicBlock bb in ins.parent.parent.BasicBlocks)
            {
                if (DataAnalysis.isLoopBody(bb) && !bb.Equals(ins.parent))
                    bodies.Add(bb);
            }

            /* If there are such basic blocks. */
            if (bodies.Count() != 0)
            {
                int num = Randomizer.SingleNumber(0, bodies.Count() - 1);
                jumptarget = bodies[num];
            }

            /* If not: we choose a random one from all the basic blocks. */
            else
            {
                List<BasicBlock> all_bbs = new List<BasicBlock>();
                foreach (BasicBlock bb in ins.parent.parent.BasicBlocks)
                {
                    /* 
                     * It shouldn't be neither the same basic block we start from,
                     * nor the one called "fake exit block".
                     */
                    if (!bb.Equals(ins.parent) && bb.getSuccessors.Count != 0)
                        all_bbs.Add(bb);
                }

                int num = Randomizer.SingleNumber(0, all_bbs.Count() - 1);
                jumptarget = all_bbs[num];
            }

            /* 
             * At this point we have a perfectly chosen jumptarget, so we should
             * split it into two pieces, so we can make a jump right in the middle
             * of the basic block, making it harder to recognize the original control
             * flow.
             */
            Instruction splithere = jumptarget.Instructions[Randomizer.SingleNumber(0, jumptarget.Instructions.Count - 1)];
            jumptarget = jumptarget.SplitAfterInstruction(splithere);

            /*
             * When we make a coditional jump here, we use a (fake) parameter with
             * a fixed value (min, max), and we must assure that the condition
             * will always end up as false.
             * 
             * These are the coditions that can satisfy this need:
             * (a, b: const, a < min, b > max)
             *  - p < <= == a    (1)
             *  - p > >= == b    (2)
             *  (These cannot be more complex because of TAC.)
             *  
             * So we get a random parameter with a fixed value, we get a random
             * constant, and we generate the condition according to these.
             */
            Variable param = Randomizer.FakeInputParameter(ins.parent.parent);
            
            /* We initialize these just to avoid further errors, it means nothing. */
            int constant = 0;
            Instruction.RelationalOperationType type = Instruction.RelationalOperationType.NotEquals;
            
            /* We decide whether we want the (1) or (2) type. */
            switch (Randomizer.SingleNumber(1, 2))
            {
                case 1:
                    /* We generate an a, where a < min. */
                    constant = Randomizer.SingleNumber(0, (int)param.fixedMin - 1);

                    /* We decide the operator type. */
                    switch (Randomizer.SingleNumber(0, 2))
                    {
                        case 0:
                            type = Instruction.RelationalOperationType.Equals;
                            break;
                        case 1:
                            type = Instruction.RelationalOperationType.Smaller;
                            break;
                        case 2:
                            type = Instruction.RelationalOperationType.SmallerOrEquals;
                            break;
                    }
                    break;

                case 2:
                    /* We generate a b, where b > max. */
                    /* TODO: a more reasonable upper bound. */
                    constant = Randomizer.SingleNumber((int)param.fixedMax + 1, (int)param.fixedMax + 1000);

                    /* We decide the operator type. */
                    switch (Randomizer.SingleNumber(0, 2))
                    {
                        case 0:
                            type = Instruction.RelationalOperationType.Equals;
                            break;
                        case 1:
                            type = Instruction.RelationalOperationType.Greater;
                            break;
                        case 2:
                            type = Instruction.RelationalOperationType.GreaterOrEquals;
                            break;
                    }
                    break;
            }

            /* Now we have everything set properly, now we can make the Conditional jump. */
            ins.MakeConditionalJump(param, constant, type, jumptarget);
        }

        /* TODO: more reasonable random numbers... */

        /// <summary>
        /// Function to make an actual fake instruction out of a Nop.
        /// </summary>
        /// <param name="ins">The nop we want to work on.</param>
        private static void GenerateFakeInstruction(Instruction ins)
        {
            /* 
             * First we get a left value.
             * If we aren't in the main route, then we cannot use NOT_INITIALIZED ones as left value.
             */
            Variable leftvalue = GetRandomLeftValue(ins, DataAnalysis.isMainRoute(ins.parent));

            /* Then we check how many variables can we use as right value. */
            List<Variable> rightvalues = GetRandomRightValues(ins, 2);

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
                        ins.MakeCopy(leftvalue, null, Randomizer.SingleNumber(0, 1000));
                    break;

                case 1:
                    int rnd = Randomizer.SingleNumber(1, 3);

                    /* We choose Full Assignment, or rightvalue is the same as leftvalue, or we are in a loop body. */
                    if (DataAnalysis.isLoopBody(ins.parent) || leftvalue.Equals(rightvalues[0]) || rnd == 3)
                    {
                        /* Here we random generate an operator. */
                        Instruction.ArithmeticOperationType op;
                        // ********* Please, use Randomizer.GetOneFromMany(...) method. Find its usage in the project.
                        switch (Randomizer.SingleNumber(0, 3))
                        {
                            case 0:
                                op = Instruction.ArithmeticOperationType.Addition;
                                break;
                            case 1:
                                op = Instruction.ArithmeticOperationType.Subtraction;
                                break;
                            case 2:
                                op = Instruction.ArithmeticOperationType.Multiplication;
                                break;
                            case 3:
                                op = Instruction.ArithmeticOperationType.Division;
                                break;
                            default:
                                /* Just to avoid the error... */
                                
                            // *********** Please, do not do it! By this you are not avoiding error, but covering it! 
                            // *********** Throw exception instead.
                                op = Instruction.ArithmeticOperationType.Addition;
                                break;
                        }
                        if (op == Instruction.ArithmeticOperationType.Addition || op == Instruction.ArithmeticOperationType.Subtraction)
                            // *********** The same problem as below - do not use 0 and 1000 as numerical values.
                            ins.MakeFullAssignment(leftvalue, leftvalue, null, Randomizer.SingleNumber(0, 1000), op);
                        else
                        {
                            // ************** Please do not use any other constants except for defined in Common class

                            // BAD USAGE:
                            int num = (int)Math.Pow(2, Randomizer.SingleNumber(1, 5));
                            
                            // EXAMPLE OF CORRECT USAGE:
                            int min = Convert.ToInt32(Math.Ceiling(Math.Log(Common.GlobalMinNumber, 2)));
                            int max = Convert.ToInt32(Math.Floor(Math.Log(Common.GlobalMaxNumber, 2)));
                            num = Randomizer.SingleNumber(min, max);

                            ins.MakeFullAssignment(leftvalue, leftvalue, null, num, op);
                        }
                    }

                    /* If we choose Unary Assignment. */
                    if (rnd == 2)
                        ins.MakeUnaryAssignment(leftvalue, rightvalues.First(), Instruction.UnaryOperationType.ArithmeticNegation);

                    /* If we choose Copy. */
                    else if (rnd == 1)
                        ins.MakeCopy(leftvalue, rightvalues.First(), null);
                    break;

                case 2:
                    if (leftvalue.Equals(rightvalues.First()))
                    {
                        Variable tmp = rightvalues[0];
                        rightvalues[0] = rightvalues[1];
                        rightvalues[1] = tmp;
                    }
                    switch (Randomizer.SingleNumber(1, 3))
                    {
                        case 1:
                            ins.MakeCopy(leftvalue, rightvalues.First(), null);
                            break;
                        case 2:
                            ins.MakeUnaryAssignment(leftvalue, rightvalues.First(), Instruction.UnaryOperationType.ArithmeticNegation);
                            break;
                        case 3:
                            /* Here we random generate an operator: + or - */
                            /* QUESTION: do we want * and / here? */

                            // ******** Now do it for Addition and Subtraction only. Multiplication and division will be realized later.
                            // ******** But mark the code with 'TODO:' in order not to forget in future

                            Instruction.ArithmeticOperationType op;
                            switch (Randomizer.SingleNumber(0, 3))
                            {
                                case 0:
                                    op = Instruction.ArithmeticOperationType.Addition;
                                    break;
                                case 1:
                                    op = Instruction.ArithmeticOperationType.Subtraction;
                                    break;
                                //case 2:
                                //    op = Instruction.ArithmeticOperationType.Multiplication;
                                //    break;
                                //case 3:
                                //    op = Instruction.ArithmeticOperationType.Division;
                                //    break;
                                default:
                                    /* Just to avoid the error... */
                                    op = Instruction.ArithmeticOperationType.Addition;
                                    break;
                            }
                            ins.MakeFullAssignment(leftvalue, rightvalues[0], rightvalues[1], null, op);
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// Returns a variable from the instruction's DeadVariables list with FREE or NOT_INITIALIZED state.
        /// </summary>
        /// <param name="ins">The actual instruction.</param>
        /// <param name="ins">Do we want NOT_INITIALIZED as well, or not?</param>
        /// <returns>A proper variable, or null, if no such exists.</returns>
        private static Variable GetRandomLeftValue(Instruction ins, bool not_init)
        {
            /* First we gather the variables with the proper state. */
            List<Variable> proper_vars = new List<Variable>();
            foreach (Variable var in ins.DeadVariables.Keys)
            {
                if (ins.DeadVariables[var] == Variable.State.Free || (not_init && ins.DeadVariables[var] == Variable.State.Not_Initialized))
                    proper_vars.Add(var);
            }

            /* If no such exists, we return null. */
            if (proper_vars.Count == 0)
                return null;

            /* If there are ones that fit our needs, then we coose one randomly. */
            else
                return proper_vars[Randomizer.SingleNumber(0, proper_vars.Count - 1)];
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
            foreach (Variable var in ins.DeadVariables.Keys)
            {
                if (ins.DeadVariables[var] == Variable.State.Free || ins.DeadVariables[var] == Variable.State.Filled)
                    proper_vars.Add(var);
            }

            /* If we have less proper variables than needed: we return as many as we have. */
            if (proper_vars.Count <= amount)
                return proper_vars;

            /* If we have more, then we pick randomly. */
            else
            {
                List<Variable> picked_vars = new List<Variable>();
                List<int> picked_nums = Randomizer.MultipleNumbers(amount, 0, proper_vars.Count - 1, false, false);
                foreach (int num in picked_nums)
                    picked_vars.Add(proper_vars[num]);
                return picked_vars;
            }
        }

        public static void GenerateNoOperations(Function func_orig)
        {
            foreach (BasicBlock bb in func_orig.BasicBlocks)
            {
                /* If this is the "fake exit block", then we shouldn't fill it with nops. */
                if (bb.getSuccessors.Count == 0)
                    continue;

                List<Instruction> insts = Common.DeepClone(bb.Instructions) as List<Instruction>;
                foreach (Instruction inst in insts)
                {
                    if (!inst.isFake)
                    {
                        int fakes_orig = FPO + 1;
                        int original_place = Randomizer.SingleNumber(0, fakes_orig);
                        if (inst.statementType == StatementTypeType.EnumValues.eConditionalJump || inst.statementType == StatementTypeType.EnumValues.eUnconditionalJump)
                            original_place = fakes_orig;
                        for (int i = 0; i < fakes_orig; i++)
                        {
                            if (i < original_place)
                                bb.Instructions.Insert(bb.Instructions.BinarySearch(inst), new Instruction(bb));
                            else if (i > original_place)
                                bb.Instructions.Insert(bb.Instructions.BinarySearch(inst) + 1, new Instruction(bb));
                        }
                    }
                }
                if (bb.Instructions.Count < fake_padding)
                {
                    int fakes = Math.Abs(Randomizer.SingleNumber(fake_padding - fake_padding_variability, fake_padding + fake_padding_variability) - bb.Instructions.Count);
                    for (int i = 0; i < fakes; i++)
                    {
                        bb.Instructions.Insert(0, new Instruction(bb));
                    }
                }
            }
        }
    }
}

//#define WORKING_IN_PROGRESS

/*
 * WARNING FOR FUTURE (1):
 * We assume that a dead pointer can only point to a non-pointer dead variable.
 * We are setting them like this, and using them like this, so be aware of this fact.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Obfuscator;

namespace Internal
{
    public partial class Routine
    {
    }


    public partial class Function
    {
        /// <summary>
        /// Gets the last basic block, which is a "fake end block"
        /// </summary>
        /// <returns>The last basic block in a function</returns>
        public BasicBlock GetLastBasicBlock()
        {
            foreach (BasicBlock bb in BasicBlocks)
                if (bb.getSuccessors.Count.Equals(0))
                    return bb;
            throw new ObfuscatorException("No 'fake end block' is found in function " + ID);
        }

        /// <summary>
        /// Gets the first basic block.
        /// </summary>
        /// <returns>The first basic block in a function.</returns>
        public BasicBlock GetFirstBasicBlock()
        {
            if (BasicBlocks.Count == 0)
                throw new ObfuscatorException("A function must have at least one basic block");
            // The first item in the BasicBlocks list is always the first basic block of the function.
            return BasicBlocks[0];
        }

        /// <summary>
        /// This function returns a reference to all basic blocks of a function, which last instruction is UnconditionalJump
        /// </summary>
        /// <returns>A list of basicblocks, which last instruction is UnconditionalJump </returns>
        public List<BasicBlock> GetUnconditionalJumps()
        {
            List<BasicBlock> list = new List<BasicBlock>();
            foreach (BasicBlock bb in BasicBlocks)
                if (bb.Instructions.Last().statementType == ExchangeFormat.StatementTypeType.EnumValues.eUnconditionalJump)
                    list.Add(bb);
            return list;
        }

        /// <summary>
        /// Gets local variable of a function by its unique identifier
        /// </summary>
        /// <param name="id">GUID of a variable</param>
        /// <returns>Local variable</returns>
        public Variable GetLocalVariableByID(string id)
        {
            foreach (Variable var in LocalVariables)
            {
                if (var.ID == id)
                    return var;
            }
            throw new ObfuscatorException("No local variable " + id + " found in function.");
        }


        public Variable NewLocalVariable(int MemoryRegionSize, Variable.Purpose purpose)
        {
            Variable var = new Variable(MemoryRegionSize, Variable.Kind.Local, purpose);
            LocalVariables.Add(var);
            return var;
        }

    }


    public partial class BasicBlock
    {
        /// <summary>
        /// Inserts an instruction to the beginning of a basic block
        /// </summary>
        /// <param name="instruction">Instruction to be inserted</param>
        public void InsertFirstInstruction(Instruction instruction)
        {
            Instructions.Insert(0, instruction);
            instruction.parent = this;
        }

        /// <summary>
        /// This is the dumb version of the InsertAfter, which inserts a BasicBlock after an unconditional jump
        /// </summary>
        /// <param name="bbTarget">The target of the unconditional jump</param>
        /// <returns>The created block</returns>
        public BasicBlock InsertAfter(BasicBlock target)
        {
            if (Successors.Count > 1) // two successors
                throw new ObfuscatorException("You cannot insert new basic block after the basic block with two successors.");

            BasicBlock newblock = new BasicBlock(parent, new Instruction(ExchangeFormat.StatementTypeType.EnumValues.eNoOperation));

            if (Successors.Count == 1)  // one successor
            {
                Successors.Clear();
                Successors.Add(newblock);
                target.Predecessors.Remove(this);
                target.Predecessors.Add(newblock);
                newblock.Predecessors.Add(this);
                newblock.Successors.Add(target);
            }
            else // no successors
            {
                Successors.Add(newblock);
                newblock.Predecessors.Add(this);
            }

            // The last instruction is modified if it was an unconditional 'goto' to match new target.ID
            RetargetLastUnconditionalGoto(newblock.ID);

            return newblock;

        }


        /// <summary>
        /// Retargets last unconditional jump (its 'goto' instruction) of a basic block to a new ID_'GUID'
        /// </summary>
        /// <param name="targetID">ID_'GUID' to be retargeted to</param>
        /// <returns>True if 'goto' has been retargeted successfully; false if the last instruction is not 'goto'</returns>
        public bool RetargetLastUnconditionalGoto(string targetID)
        {
            if (Instructions.Last().statementType != ExchangeFormat.StatementTypeType.EnumValues.eUnconditionalJump)
                return false;
            else
            {
                string resultString = null;
                resultString = Regex.Match(Instructions.Last().TACtext, @"\bID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b").Value;
                Instructions.Last().TACtext = Instructions.Last().TACtext.Replace(resultString, targetID);
                return true;
            }
        }
    }


    public partial class Instruction
    {
        /// <summary>
        /// Gets a list of the instructions followed by the given instruction in a control flow (directly preceding in CFG)
        /// </summary>
        /// <returns>A list of instructions (or an empty list if no such found)</returns>
        public List<Instruction> GetPrecedingInstructions()
        {
            List<Instruction> preceding = new List<Instruction>();
            if(parent == null)
                throw new ObfuscatorException("Instruction is not contained in a basic block. Instruction's 'parent' propery is null.");
            if (parent.Instructions.Contains(this))
            {
                if (parent.Instructions.First().Equals(this))
                    foreach (BasicBlock bb in parent.getPredecessors)
                    {
                        if (bb.Instructions.Count == 0)
                            throw new ObfuscatorException("No instructions found in basic block" + bb.ID);
                        preceding.Add(bb.Instructions.Last());
                    }
                else
                    preceding.Add(parent.Instructions[parent.Instructions.BinarySearch(this) - 1]);
            }
            else
                throw new ObfuscatorException("The instruction is not properly linked to its parent. It is not contained in 'parent.Instructions' list.");
            return preceding;
        }

        /// <summary>
        /// Gets a list of the instructions following the given instruction in a control flow (directly following in CFG)
        /// </summary>
        /// <returns>A list of instructions (or empty list if no such found)</returns>
        public List<Instruction> GetFollowingInstructions()
        {
            List<Instruction> following = new List<Instruction>();
            if (parent == null)
                throw new ObfuscatorException("Instruction is not contained in a basic block. Instruction's 'parent' propery is null.");
            if (parent.Instructions.Contains(this))
            {
                if (parent.Instructions.Last().Equals(this))
                    foreach (BasicBlock bb in parent.getSuccessors)
                    {
                        if (bb.Instructions.Count == 0)
                            throw new ObfuscatorException("No instructions found in basic block" + bb.ID);
                        following.Add(bb.Instructions.First());
                    }
                else
                    following.Add(parent.Instructions[parent.Instructions.BinarySearch(this) + 1]);
            }
            else
                throw new ObfuscatorException("The instruction is not properly linked to its parent. It is not contained in 'parent.Instructions' list.");
            return following;
        }

        /// <summary>
        /// Gets a list of unsafe variables, that contains variables which never become 'dead'
        /// </summary>
        /// <returns>A list of variables</returns>
        public List<Variable> GetUnsafeVariables()
        {
            List<Variable> unsafeVar = new List<Variable>();
            if (statementType == ExchangeFormat.StatementTypeType.EnumValues.ePointerAssignment)
            {
                string resultString = Regex.Match(TACtext, @"& [vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$").Value;
                if (string.IsNullOrEmpty(resultString))
                    return unsafeVar;
                resultString = Regex.Match(TACtext, @"[vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$").Value;
                unsafeVar.Add(this.parent.parent.GetLocalVariableByID(resultString));
            }
            return unsafeVar;
        }

        /// <summary>
        /// Determines the new state of a dead variable (a pointer) as a result of the instruction
        /// </summary>
        /// <param name="variable">A variable or a pointer</param>
        /// <returns>A list of states. For variable: 1 - variable, 2 - empty. For pointer: 1-pointer, 2 - variable. Empty list if nothing has changed.</returns>
        public List<Variable.State> GetChangedStates(Variable variable)
        {
            if (string.IsNullOrWhiteSpace(TACtext))
                throw new ObfuscatorException("TAC text is empty. Instruction: " + ID);
            if(RefVariables.Count==0 || !RefVariables.Contains(variable))
                throw new ObfuscatorException("No referenced variables found in instruction " + ID);
            List<Variable.State> var_states = new List<Variable.State>();
            string right=string.Empty, left=string.Empty;
            if (TACtext.Split('=').Length == 2)
            {
                right = TACtext.Split('=')[0];
                left = TACtext.Split('=')[0];
            }
            switch (statementType)
            {
                case ExchangeFormat.StatementTypeType.EnumValues.eFullAssignment:
                case ExchangeFormat.StatementTypeType.EnumValues.eUnaryAssignment:
                case ExchangeFormat.StatementTypeType.EnumValues.eCopy:
                        if (Regex.IsMatch(right, variable.ID, RegexOptions.None)&&!Regex.IsMatch(left, variable.ID, RegexOptions.None))
                            var_states.Add(Variable.State.Free);
                        else if (Regex.IsMatch(left, variable.ID, RegexOptions.None))
                            var_states.Add(Variable.State.Filled);
                    break;
                case ExchangeFormat.StatementTypeType.EnumValues.eConditionalJump:
                        var_states.Add(Variable.State.Free);
                    break;
                case ExchangeFormat.StatementTypeType.EnumValues.ePointerAssignment:
                    if (left.Contains("&"))
                        throw new ObfuscatorException("Instruction type '&a=p' is not supported. Please use 'p=&a' instead.");
                    else if (right.Contains("&"))
                    {
                        // Nothing changed
                        if (Regex.IsMatch(right, variable.ID, RegexOptions.None) && !variable.pointer)
                            break;
                        // Pointer is filled
                        else if (Regex.IsMatch(left, variable.ID, RegexOptions.None) && variable.pointer)
                        {
                            var_states.Add(Variable.State.Filled);
                        }
                        else
                            throw new ObfuscatorException("GetChangedStates could not parse 'p=&a' instruction type. Possible errors: 'a' is a poiner; 'p' is not a pointer; incorrect TAC text.");
                    }
                    else if (left.Contains("*"))
                    {
                        // Variable state is free
                        if (Regex.IsMatch(right, variable.ID, RegexOptions.None) && !variable.pointer)
                            var_states.Add(Variable.State.Free);
                        // Pointer is free, variable is filled
                        else if (Regex.IsMatch(left, variable.ID, RegexOptions.None) && variable.pointer)
                        {
                            var_states.Add(Variable.State.Free);
                            var_states.Add(Variable.State.Filled);
                        }
                        else
                            throw new ObfuscatorException("GetChangedStates could not parse '*p=a' instruction type. Possible error: 'a' is a poiner or 'p' is not a pointer; incorrect TAC text.");
                    }
                    else if (right.Contains("*"))
                    {
                        // Pointer is free, variable is free
                        if (Regex.IsMatch(right, variable.ID, RegexOptions.None) && variable.pointer)
                        {
                            var_states.Add(Variable.State.Free);
                            var_states.Add(Variable.State.Free);
                        }
                        // Variable is filled
                        else if (Regex.IsMatch(left, variable.ID, RegexOptions.None) && !variable.pointer)
                        {
                            var_states.Add(Variable.State.Filled);
                        }
                        else
                            throw new ObfuscatorException("GetChangedStates could not parse 'a=*p' instruction type. Possible error: 'a' is a poiner or 'p' is not a pointer; incorrect TAC text.");
                    }
                    else
                        throw new ObfuscatorException("GetChangedStates: problem in parsing TAC text of PointerAssignment instruction type. Instruction: " + ID);
                    break;
                case ExchangeFormat.StatementTypeType.EnumValues.eIndexedAssignment:
                    throw new ObfuscatorException("This statement type is not supported.");
                case ExchangeFormat.StatementTypeType.EnumValues.eProcedural:
                    if(variable.pointer)
                        throw new ObfuscatorException("GetChangedStates: pointers are not supported in Procedural instruction type.");
                    else if((Regex.IsMatch(TACtext, "param", RegexOptions.None) || Regex.IsMatch(TACtext, "return", RegexOptions.None)) && Regex.IsMatch(TACtext, RefVariables[0].ID, RegexOptions.None))
                        var_states.Add(Variable.State.Free);
                    else if (Regex.IsMatch(TACtext, "retrieve", RegexOptions.None) && Regex.IsMatch(TACtext, RefVariables[0].ID, RegexOptions.None))
                        var_states.Add(Variable.State.Filled);
                    break;
                case ExchangeFormat.StatementTypeType.EnumValues.eUnconditionalJump:
                case ExchangeFormat.StatementTypeType.EnumValues.eNoOperation:
                case ExchangeFormat.StatementTypeType.EnumValues.Invalid:
                default:
                    throw new ObfuscatorException("This statement type cannot change states of 'dead' variables.");
            }
            return var_states;
        }


        public void MakeFullAssignment(Variable left_value, Variable right_value1, Variable right_value2, int right_value_int, Instruction.ArithmeticOperationType operation)
        {
            if ((right_value2 == null && right_value_int == null) || (right_value2 != null && right_value_int != null) || left_value == null || right_value1 == null || operation == 0)
                throw new ObfuscatorException("MakeFullAssignment: wrong parameter passing.");
            string left1 = left_value.name;
            string right1 = right_value1.name;
            string right2 = right_value_int == null ? right_value2.name : right_value_int.ToString();
            string op;
            switch (operation)
            {
                case ArithmeticOperationType.Addition:
                    op = "+";
                    break;
                case ArithmeticOperationType.Subtraction:
                    op = "-";
                    break;
                case ArithmeticOperationType.Multiplication:
                    op = "*";
                    break;
                case ArithmeticOperationType.Division:
                    op = @"/";
                    break;
                default:
                    throw new ObfuscatorException("MakeFullAssignment: unsupported operation type.");
            }
            RefVariables.Clear();
            RefVariables.Add(left_value);
            RefVariables.Add(right_value1);
            if (right_value_int == null)
                RefVariables.Add(right_value2);
            statementType = ExchangeFormat.StatementTypeType.EnumValues.eFullAssignment;
            TACtext = string.Join(" ", left1, ":=", right1, op, right2);
        }


        public void MakeCopy(Variable left_value, Variable right_value, int right_value_int)
        {
            if (left_value == null || (right_value == null && right_value_int == null) || (right_value != null && right_value_int != null))
                throw new ObfuscatorException("MakeCopy: wrong parameter passing.");
            RefVariables.Clear();
            RefVariables.Add(left_value);
            if (right_value_int == null)
                RefVariables.Add(right_value);
            statementType = ExchangeFormat.StatementTypeType.EnumValues.eCopy;
            TACtext = right_value_int == null ? string.Join(" ", left_value.name, ":=", right_value.name) : string.Join(" ", left_value.name, ":=", right_value_int);
        }


#if !WORKING_IN_PROGRESS

        /*
         * When we modify a fake instruction, we change the states in it, so we
         * have to update the dead variables in the following instructions.
         */
        /// <summary>
        /// Refreshes the state of all following instructions' dead variables.
        /// </summary>
        public void RefreshNext()
        {
            /*
             * For every used dead variable in the instruction we should first determine
             * its new state, then we could push it's (changed) state through all the
             * following instructions, so it will get the appropriate state everywhere.
             */
            foreach (Variable var in RefVariables)
            {
                if (DeadVariables.ContainsKey(var))
                {
                    List<Variable.State> states = GetChangedStates(var);

                    if (states.Count() == 0)
                    {
                        /* Nothing has changed. */
                        return;
                    }

                    /* First we set the state of that used dead variable. */
                    DeadVariables[var] = states.First();

                    /* Then we tell its (changed) state to the following instructions. */
                    foreach (Instruction ins in GetFollowingInstructions())
                        ins.RefreshNext(var, DeadVariables[var]);
                }
                else if (DeadPointers.ContainsKey(var))
                {
                    List<Variable.State> states = GetChangedStates(var);

                    /* First we set the state of that used dead pointer. */
                    DeadPointers[var].State = states.First();

                    /* Then we tell its (changed) state to the following instructions. */
                    foreach (Instruction ins in GetFollowingInstructions())
                        ins.RefreshNext(var, DeadPointers[var].State);

                    /* 
                     * We set the state of the dead variable pointed by the dead pointer.
                     * 
                     * WARNING FOR FUTURE: (1)
                     */ 
                    if (states.Count() > 1)
                        DeadVariables[DeadPointers[var].PointsTo] = states[1];

                    /* Then we tell its (changed) state to the following instructions. */
                    foreach (Instruction ins in GetFollowingInstructions())
                        ins.RefreshNext(DeadPointers[var].PointsTo, DeadVariables[DeadPointers[var].PointsTo]);
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
        private void RefreshNext(Variable var, Variable.State state)
        {
            /*
             * If this instruction uses this variable as well, or this instruction
             * uses a pointer that points to this variable then its state must not
             * be changed, because it's perfect as it is right now.
             */
            if (RefVariables.Contains(var))
            {
                /* This instruction uses this variable. */
                return;
            }

            /*
             * We have to do anything only if the variable is in this instruction's
             * DeadVariables list, and it's state differs from the new state.
             */
            if (DeadVariables.ContainsKey(var)      // This variable is dead.
                && DeadVariables[var] != state)     // The states differ.
            {
                foreach (Variable v in RefVariables)
                {
                    if (DeadPointers.ContainsKey(v) && DeadPointers[v].PointsTo.Equals(var))
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
                state = check_preceding_states(var, state);
                
                DeadVariables[var] = state;
                foreach (Instruction ins in GetFollowingInstructions())
                    ins.RefreshNext(var, state);
            }

            /*
             * Another case for doing something when we pass a dead pointer with changed state.
             * In this case we do not have to look for variables pointing to that one.
             * 
             * WARNING FOR FUTURE: (1)
             */
            if (DeadPointers.ContainsKey(var)           // This is a dead pointer.
                && DeadPointers[var].State != state)    // The states differ.
            {
                /* Just like in the previous case, we have to check all the preceding states for collisions. */
                state = check_preceding_states(var, state);

                DeadPointers[var].State = state;
                foreach (Instruction ins in GetFollowingInstructions())
                    ins.RefreshNext(var, state);
            }
        }

        private Variable.State check_preceding_states(Variable var, Variable.State state)
        {
            /* So we get all states from the preceding instructions. */
            List<Variable.State> preceding_states = new List<Variable.State>();
            foreach (Instruction ins in GetPrecedingInstructions())
            {
                if (ins.DeadVariables.ContainsKey(var) && !preceding_states.Contains(ins.DeadVariables[var]))
                    preceding_states.Add(ins.DeadVariables[var]);
                else if (ins.DeadPointers.ContainsKey(var) && !preceding_states.Contains(ins.DeadPointers[var].State))
                    preceding_states.Add(ins.DeadPointers[var].State);
            }

            /* We only have to do anything if there are more than one of those. */
            if (preceding_states.Count() > 1)
            {
                switch (state)
                {
                    case Variable.State.Free:
                        if (preceding_states.Contains(Variable.State.Not_Initialized))
                        {
                            /* FREE meets NOT_INITIALIZED */
                            state = Variable.State.Not_Initialized;
                        }
                        else if (preceding_states.Contains(Variable.State.Filled))
                        {
                            /* FREE meets FILLED */
                            state = Variable.State.Filled;
                        }
                        break;

                    case Variable.State.Filled:
                        if (preceding_states.Contains(Variable.State.Not_Initialized))
                        {
                            /* 
                             * FILLED meets NOT_INITIALIZED
                             * 
                             * TODO: What should we do in these cases?
                             */
                            throw new ObfuscatorException("Unhandled state collision, do something!");
                        }
                        break;
                }
            }
            return state;
        }
#endif

    }


    public partial class Variable
    {

    }
}

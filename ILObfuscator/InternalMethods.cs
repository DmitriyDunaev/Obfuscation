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
        /// Gets the "fake exit block" (it is not the last block in the list) of a function
        /// </summary>
        /// <returns>The "fake exit block"</returns>
        public BasicBlock GetFakeExitBasicBlock()
        {
            return BasicBlocks.Find(x => x.getSuccessors.Count == 0);
        }


        /// <summary>
        /// Gets the entrance basic block of a function. By convention it is the first basic block in a list 
        /// </summary>
        /// <returns>The entrance basic block</returns>
        public BasicBlock GetEntranceBasicBlock()
        {
            return BasicBlocks[0];
        }


        /// <summary>
        /// Creates a new fake local variable and adds it to the function
        /// </summary>
        /// <param name="purpose">Purpose of creation (choose from enumeration)</param>
        /// <param name="MemoryRegionSize">Memory region size; 4 bytes for integer</param>
        /// <returns>The created variable</returns>
        public Variable NewLocalVariable(Variable.Purpose purpose, Common.MemoryRegionSize size)
        {
            Variable var = new Variable(Variable.Kind.Local, purpose, size);
            LocalVariables.Add(var);
            return var;
        }


        /// <summary>
        /// Creates a new fake input parameter and adds it to the function
        /// </summary>
        /// <param name="min_value">Fixed minimum value (positive integer)</param>
        /// <param name="max_value">Fixed maximum value (positive integer)</param>
        /// <returns>A variable</returns>
        public Variable NewFakeInputParameter(int? min_value, int? max_value)
        {
            if (min_value.HasValue && max_value.HasValue && min_value > max_value)
                throw new ObfuscatorException("Wrong parameter passing: minvalue cannot exceed maxvalue.");
            if (!min_value.HasValue && !max_value.HasValue)
                throw new ObfuscatorException("One of values (FixedMin or FixedMax) should be a not-null value.");
            if ((min_value.HasValue && min_value.Value < 0) || (max_value.HasValue && max_value.Value < 0))
                throw new ObfuscatorException("At present time only positive integer FixedMin and FixedMax are supported.");
            Variable fake_input = new Variable(Variable.Kind.Input, Variable.Purpose.Fake, Common.MemoryRegionSize.Integer, min_value, max_value);
            LocalVariables.Add(fake_input);
            return fake_input;
        }
    }



    public partial class BasicBlock
    {
        /// <summary>
        /// Splits the basic block into two after the given instruction (+ handles successor-predecessor links)
        /// </summary>
        /// <param name="inst">Instruction to split after</param>
        /// <returns>The new basic block (the second after splitting)</returns>
        public BasicBlock SplitAfterInstruction(Instruction inst)
        {
            if (!inst.parent.Equals(this) || !this.Instructions.Contains(inst))
                throw new ObfuscatorException("The instruction does not belong to the given basic block.");
            if (inst.statementType == ExchangeFormat.StatementTypeType.EnumValues.eConditionalJump)
                throw new ObfuscatorException("You cannot split a basic block after the conditional jump.");
            BasicBlock newBB = new BasicBlock(parent);
            // Relinking the blocks
            foreach (BasicBlock bb in Successors)
                newBB.LinkToSuccessor(bb);
            this.LinkToSuccessor(newBB, true);
            List<Instruction> move = Instructions.GetRange(Instructions.BinarySearch(inst) + 1, Instructions.Count - Instructions.BinarySearch(inst) - 1);
            // If something is moved, then clear NoOperation default instruction in the new basic block
            if (move.Count > 0)
                newBB.Instructions.Clear();
            newBB.Instructions.AddRange(move);
            Instructions.RemoveRange(Instructions.BinarySearch(inst) + 1, Instructions.Count - Instructions.BinarySearch(inst) - 1);
            foreach (Instruction ins in newBB.Instructions)
                ins.parent = newBB;
            // We have to retarget the last unconditional GOTO to a new basic block
            if (inst.statementType == ExchangeFormat.StatementTypeType.EnumValues.eUnconditionalJump)
                Instructions.Last().TACtext = Instructions.Last().TACtext.Replace(Regex.Match(Instructions.Last().TACtext, @"\bID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b").Value, newBB.ID);
            return newBB;
        }


        /// <summary>
        /// Links the basic block to a new successor (sets this.Successor and successor.Predecessor properties)
        /// </summary>
        /// <param name="successor">A new successor basic block</param>
        /// <param name="clear">If true - clears all other successor-predecessor links of the basic block before linking to a new successor</param>
        public void LinkToSuccessor(BasicBlock successor, bool clear = false)
        {
            if (Successors.Count > 2)
                throw new ObfuscatorException("Basic block cannot have more than 2 successors.");
            if (clear)
            {
                foreach (BasicBlock next in Successors)
                    next.Predecessors.Remove(this);
                Successors.Clear();
            }
            Successors.Add(successor);
            successor.Predecessors.Add(this);
        }


        /// <summary>
        /// Clones a BasicBlock with its instructions and successors
        /// </summary>
        /// <param name="polyrequired">If true, the polyRequired property is set</param>
        /// <returns>The created BasicBlock</returns>
    //    public BasicBlock Clone(bool polyrequired = false)
    //    {
    //        /// Creating the clone
    //        BasicBlock clone = new BasicBlock(parent);

    //        clone.Instructions.Clear();

    //        /// Cloning the instructions
    //        foreach (Instruction ins in Instructions)
    //        {
    //            Instruction i = new Instruction(ins, clone);
    //            clone.Instructions.Add(i);
    //        }
    //        foreach (Instruction ins in clone.Instructions)
    //            ins.parent = clone;

    //        /// Setting the instructions to polyrequired, if needed
    //        if (polyrequired)
    //            foreach (Instruction ins in clone.Instructions)
    //                ins.polyRequired = true;

    //        /// Setting the successors
    //        foreach (BasicBlock bb in Successors)
    //            clone.LinkToSuccessor(bb);

    //        return clone;
    //    }
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
            if (parent == null)
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
                unsafeVar.Add(this.parent.parent.LocalVariables.Find(x => x.ID == resultString));
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
            if (RefVariables.Count == 0 || !RefVariables.Contains(variable))
                throw new ObfuscatorException("No referenced variables found in instruction " + ID);
            List<Variable.State> var_states = new List<Variable.State>();
            string right = string.Empty, left = string.Empty;
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
                    if (Regex.IsMatch(right, variable.ID, RegexOptions.None) && !Regex.IsMatch(left, variable.ID, RegexOptions.None))
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
                    if (variable.pointer)
                        throw new ObfuscatorException("GetChangedStates: pointers are not supported in Procedural instruction type.");
                    else if ((Regex.IsMatch(TACtext, "param", RegexOptions.None) || Regex.IsMatch(TACtext, "return", RegexOptions.None)) && Regex.IsMatch(TACtext, RefVariables[0].ID, RegexOptions.None))
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
        

        /// <summary>
        /// Modifies the TAC text of instruction: replaces numerical constant to variable name
        /// </summary>
        /// <param name="variable">Variable containing constant</param>
        /// <param name="value">Constant number to be replaced</param>
        public void ModifyConstInstruction(Variable variable, int? value)
        {
            if (variable == null || value == null)
                throw new ObfuscatorException("Wrong parameter passing.");
            RefVariables.Add(variable);
            string TACtext_new = string.Empty;
            for (int i = 0; i < TACtext.Split(' ').Length; i++)
            {
                if (TACtext.Split(' ')[i] != value.ToString())
                    TACtext_new = string.Join(" ", TACtext_new, TACtext.Split(' ')[i]);
                else
                    TACtext_new = string.Join(" ", TACtext_new, variable.name);
            }
            TACtext = TACtext_new.Trim();
        }


        /// <summary>
        /// Extracts the variable from a condition
        /// </summary>
        /// <returns>The extracted variable</returns>
        internal Variable GetVarFromCondition()
        {
            // TODO;

            return RefVariables.First();
        }

        /// <summary>
        /// Extracts the relop from a condition
        /// </summary>
        /// <returns>The extracted variable</returns>
        internal Instruction.RelationalOperationType GetRelopFromCondition()
        {
            // TODO;

            return Instruction.RelationalOperationType.Equals;
        }

        /// <summary>
        /// Extracts the constant from a condition
        /// </summary>
        /// <returns>The extracted variable</returns>
        internal int GetConstFromCondition()
        {
            // TODO;

            return 100;
        }

        /// <summary>
        /// Gets the true case successor
        /// </summary>
        /// <returns>The true cuccessor BasicBlock</returns>
        internal BasicBlock GetTrueSucc()
        {
            return parent.getSuccessors.First();
        }

        /// <summary>
        /// Gets the false case successor
        /// </summary>
        /// <returns>The false cuccessor BasicBlock</returns>
        internal BasicBlock GetFalseSucc()
        {
            return parent.getSuccessors.Last();
        }

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
             * We have to do anything only if the variable is in this instruction is
             * dead, and it's state differs from the new state.
             */
            if (DeadVariables.ContainsKey(var)      // This variable is dead.
                && DeadVariables[var] != state)     // The state differs.
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
    }


    public partial class Variable
    {

    }
}

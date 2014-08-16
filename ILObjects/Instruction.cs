using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExchangeFormat;

namespace Objects
{
    [Serializable]
    public partial class Instruction : IComparable, IValidate
    {
        public enum ProceduralType
        {
            Param = 0,
            Call = 1,
            Return = 2,
            Retrieve = 3
        }

        public enum ArithmeticOperationType
        {
            Addition = 0,
            Subtraction = 1,
            Multiplication = 2,
            Division = 3,
            Modulo = 4
        }

        public enum PoinerType
        {
            Variable_EQ_AddressOfObject = 0,
            Variable_EQ_PointedObject = 1,
            PointedObject_EQ_Variable = 2,
            PointedObject_EQ_Number = 3
        }

        public enum UnaryOperationType
        {
            ArithmeticNegation = 0,
            LogicalNegation = 1
        }

        public enum RelationalOperationType
        {
            Equals = 0,
            NotEquals = 1,
            Greater = 2,
            GreaterOrEquals = 3,
            Smaller = 4,
            SmallerOrEquals = 5
        }

        public enum ConditionType
        {
            AlwaysTrue = 0,
            AlwaysFalse = 1,
            Random = 3
        }

        //Attributes
        public BasicBlock parent { get; set; }
        private IDManager _ID;
        public string ID
        {
            get { return _ID.ToString(); }
        }
        public Common.StatementType statementType { get; set; }
        public string TACtext { get; set; }
        public bool isFake { get; private set; }

        public List<Variable> RefVariables = new List<Variable>();

        public Dictionary<Variable, Variable.State> DeadVariables = new Dictionary<Variable, Variable.State>();
        public Dictionary<Variable, PointerData> DeadPointers = new Dictionary<Variable, PointerData>();

        //Constructors
        public Instruction(InstructionType instr, BasicBlock par)
        {
            parent = par;
            _ID = new IDManager(instr.ID.Value);
            statementType = (Common.StatementType)instr.StatementType.EnumerationValue;
            TACtext = instr.Value;
            isFake = false;
            if (instr.RefVars.Exists())
            {
                foreach (string vid in instr.RefVars.Value.Split(' '))
                {
                    // Searching in local variables
                    foreach (Variable var in parent.parent.LocalVariables)
                    {
                        if (var.ID.Equals(vid))
                            RefVariables.Add(var);
                    }
                    // Searching in global variables
                    foreach (Variable var in parent.parent.parent.GlobalVariables)
                    {
                        if (var.ID.Equals(vid))
                            RefVariables.Add(var);
                    }
                }
                if (!instr.RefVars.Value.Split(' ').Length.Equals(RefVariables.Count))
                    throw new ValidatorException("Referenced variable was not found. Instruction: " + instr.ID.Value);
            }
        }

        public Instruction(BasicBlock parent)
        {
            _ID = new IDManager();
            isFake = true;
            this.parent = parent;
            statementType = Common.StatementType.NoOperation;
            TACtext = "nop";
        }


        public void ResetID()
        {
            _ID = new IDManager();
        }

        //Interface methods
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            // Equal
            if ((obj as Instruction).ID.Equals(ID))
                return 0;
            foreach (Instruction instr in parent.Instructions)
            {
                if (instr.ID.Equals(ID))
                    return -1;
                if (instr.ID.Equals((obj as Instruction).ID))
                    return 1;
            }
            throw new ArgumentException("Comparison error! Instruction " + (obj as Instruction).ID + " is not found in basic block.");
        }

        //Overloaded methods
        public override bool Equals(object obj)
        {
            return (obj as Instruction) == null ? base.Equals(obj) : ((Instruction)obj).ID == ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }


        // METHODS


        /// <summary>
        /// Gets a list of the instructions followed by the given instruction in a control flow (directly preceding in CFG)
        /// </summary>
        /// <returns>A list of instructions (or an empty list if no such found)</returns>
        public List<Instruction> GetPrecedingInstructions()
        {
            List<Instruction> preceding = new List<Instruction>();
            if (parent == null)
                throw new ObjectException("Instruction is not contained in a basic block. Instruction's 'parent' propery is null.");
            if (parent.Instructions.Contains(this))
            {
                if (parent.Instructions.First().Equals(this))
                    foreach (BasicBlock bb in parent.getPredecessors)
                    {
                        if (bb.Instructions.Count == 0)
                            throw new ObjectException("No instructions found in basic block" + bb.ID);
                        preceding.Add(bb.Instructions.Last());
                    }
                else
                    preceding.Add(parent.Instructions[parent.Instructions.BinarySearch(this) - 1]);
            }
            else
                throw new ObjectException("The instruction is not properly linked to its parent. It is not contained in 'parent.Instructions' list.");
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
                throw new ObjectException("Instruction is not contained in a basic block. Instruction's 'parent' propery is null.");
            if (parent.Instructions.Contains(this))
            {
                if (parent.Instructions.Last().Equals(this))
                    foreach (BasicBlock bb in parent.getSuccessors)
                    {
                        if (bb.Instructions.Count == 0)
                            throw new ObjectException("No instructions found in basic block" + bb.ID);
                        following.Add(bb.Instructions.First());
                    }
                else
                    following.Add(parent.Instructions[parent.Instructions.BinarySearch(this) + 1]);
            }
            else
                throw new ObjectException("The instruction is not properly linked to its parent. It is not contained in 'parent.Instructions' list.");
            return following;
        }


        /// <summary>
        /// Gets a list of unsafe variables, that contains variables which never become 'dead'
        /// </summary>
        /// <returns>A list of variables</returns>
        public List<Variable> GetUnsafeVariables()
        {
            List<Variable> unsafeVar = new List<Variable>();
            if (statementType == Common.StatementType.PointerAssignment)
            {
                string resultString = Regex.Match(TACtext, @"& [vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$").Value;
                if (string.IsNullOrEmpty(resultString))
                    return unsafeVar;
                resultString = Regex.Match(TACtext, @"ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$").Value;
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
                throw new ObjectException("TAC text is empty. Instruction: " + ID);
            if (RefVariables.Count == 0 || !RefVariables.Contains(variable))
                throw new ObjectException("No referenced variables found in instruction " + ID);
            List<Variable.State> var_states = new List<Variable.State>();
            string right = string.Empty, left = string.Empty;
            if (TACtext.Split('=').Length == 2)
            {
                right = TACtext.Split('=')[1];
                left = TACtext.Split('=')[0];
            }
            switch (statementType)
            {
                case Common.StatementType.FullAssignment:
                case Common.StatementType.UnaryAssignment:
                case Common.StatementType.Copy:
                    if (Regex.IsMatch(right, variable.ID, RegexOptions.None) && !Regex.IsMatch(left, variable.ID, RegexOptions.None))
                        var_states.Add(Variable.State.Free);
                    else if (Regex.IsMatch(left, variable.ID, RegexOptions.None))
                        var_states.Add(Variable.State.Filled);
                    break;
                case Common.StatementType.ConditionalJump:
                    var_states.Add(Variable.State.Free);
                    break;
                case Common.StatementType.PointerAssignment:
                    if (left.Contains("&"))
                        throw new ObjectException("Instruction type '&a=p' is not supported. Please use 'p=&a' instead.");
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
                            throw new ObjectException("GetChangedStates could not parse 'p=&a' instruction type. Possible errors: 'a' is a poiner; 'p' is not a pointer; incorrect TAC text.");
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
                            throw new ObjectException("GetChangedStates could not parse '*p=a' instruction type. Possible error: 'a' is a poiner or 'p' is not a pointer; incorrect TAC text.");
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
                            throw new ObjectException("GetChangedStates could not parse 'a=*p' instruction type. Possible error: 'a' is a poiner or 'p' is not a pointer; incorrect TAC text.");
                    }
                    else
                        throw new ObjectException("GetChangedStates: problem in parsing TAC text of PointerAssignment instruction type. Instruction: " + ID);
                    break;
                case Common.StatementType.IndexedAssignment:
                    throw new ObjectException("This statement type is not supported.");
                case Common.StatementType.Procedural:
                    if (variable.pointer)
                        throw new ObjectException("GetChangedStates: pointers are not supported in Procedural instruction type.");
                    else if ((Regex.IsMatch(TACtext, "param", RegexOptions.None) || Regex.IsMatch(TACtext, "return", RegexOptions.None)) && Regex.IsMatch(TACtext, RefVariables[0].ID, RegexOptions.None))
                        var_states.Add(Variable.State.Free);
                    else if (Regex.IsMatch(TACtext, "retrieve", RegexOptions.None) && Regex.IsMatch(TACtext, RefVariables[0].ID, RegexOptions.None))
                        var_states.Add(Variable.State.Filled);
                    break;
                case Common.StatementType.UnconditionalJump:
                case Common.StatementType.NoOperation:
                default:
                    throw new ObjectException("This statement type cannot change states of 'dead' variables.");
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
                throw new ObjectException("Wrong parameter passing.");
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

        /// I needed this functions, to somehow extract the variable, the relop and the constant from a condition.
        /// These are only test functions, the real one(s) are still has to be done.
        /// If you can do this, it will be a huge help for me.

        /// <summary>
        /// Extracts the left variable from a condition
        /// </summary>
        /// <returns>The extracted variable</returns>
        public Variable GetVarFromCondition()
        {
            // TODO;

            return RefVariables.First();
        }

        /// <summary>
        /// Extracts the right variable from a condition
        /// </summary>
        /// <returns>The extracted variable</returns>
        public Variable GetRightVarFromCondition()
        {
            // TODO;

            return RefVariables.Last();
        }

        /// <summary>
        /// Extracts the relop from a condition
        /// </summary>
        /// <returns>The extracted variable</returns>
        public Instruction.RelationalOperationType GetRelopFromCondition()
        {
            Validate();
            Instruction.RelationalOperationType relop = 0;
            string relop_sign = Regex.Match(TACtext, @"( == )|( != )|( > )|( >= )|( < )|( <= )").Value.Trim();
            switch (relop_sign)
            {
                case "==":
                    relop = Instruction.RelationalOperationType.Equals;
                    break;
                case "!=":
                    relop = Instruction.RelationalOperationType.NotEquals;
                    break;
                case ">":
                    relop = Instruction.RelationalOperationType.Greater;
                    break;
                case ">=":
                    relop = Instruction.RelationalOperationType.GreaterOrEquals;
                    break;
                case "<":
                    relop = Instruction.RelationalOperationType.Smaller;
                    break;
                case "<=":
                    relop = Instruction.RelationalOperationType.SmallerOrEquals;
                    break;
                default:
                    throw new ObjectException("Cannot parse relational operator.");
            }
            return relop;
        }

        /// <summary>
        /// Extracts the constant from a condition
        /// </summary>
        /// <returns>The extracted variable</returns>
        public int? GetConstFromCondition()
        {
            Validate();
            System.Collections.Specialized.StringCollection refvarIDs = new System.Collections.Specialized.StringCollection();
            Match matchResult = Regex.Match(TACtext, "ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}", RegexOptions.None);
            while (matchResult.Success)
            {
                refvarIDs.Add(matchResult.Value);
                matchResult = matchResult.NextMatch();
            }
            if (refvarIDs.Count == 3)
                return null;
            else
                return Convert.ToInt32(TACtext.Split(' ')[3]);
        }

        /// <summary>
        /// Gets the true case successor
        /// </summary>
        /// <returns>The true cuccessor BasicBlock</returns>
        public BasicBlock GetTrueSucc()
        {
            /// It's not a convention, only the test version of the function.
            /// Todo: Checking the tac text -> the target of the goto will be the true
            return parent.getSuccessors.First();
        }

        /// <summary>
        /// Gets the false case successor
        /// </summary>
        /// <returns>The false cuccessor BasicBlock</returns>
        public BasicBlock GetFalseSucc()
        {
            /// Todo: Returning not the true one.
            return parent.getSuccessors.Last();
        }


    }
}

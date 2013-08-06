using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExchangeFormat;
using System.Text.RegularExpressions;

namespace Obfuscator
{
    public partial class Routine: IValidate
    {
        // Attributes
        private string description;
        public List<Variable> GlobalVariables = new List<Variable>();
        public List<Function> Functions = new List<Function>();

        // Constructor
        public Routine(Exchange doc)
        {
            description = doc.Routine[0].Description.Value;
            if (doc.Routine[0].Global.Exists)
                foreach (VariableType var in doc.Routine[0].Global[0].Variable)
                    GlobalVariables.Add(new Variable(var, Variable.Kind.Global));
            foreach (FunctionType function in doc.Routine[0].Function)
                Functions.Add(new Function(function, this));
        }
    }


    public partial class Function: IValidate
    {
        // Attributes
        public Routine parent { get; private set; }
        private IDManager _ID;
        public string ID
        {
            get { return _ID.ToString(); }
        }
        public string globalID { get; private set; }

        public CalledFromType.EnumValues calledFrom { get; private set; }
        
        public List<Variable> LocalVariables = new List<Variable>();
        public List<BasicBlock> BasicBlocks = new List<BasicBlock>();

        // Constructor
        public Function(FunctionType function, Routine par)
        {
            _ID = new IDManager(function.ID.Value);
            parent = par;
            calledFrom = function.CalledFrom.EnumerationValue;
            globalID = function.GlobalID.Value;
            // Collecting all variables
            if (function.Local.Exists)
            {
                foreach (VariableType var in function.Local[0].Variable)
                {
                    if (function.RefInputVars.Exists() && function.RefInputVars.Value.Split(' ').Contains(var.ID.Value))
                        LocalVariables.Add(new Variable(var, Variable.Kind.Input));
                    else if (function.RefOutputVars.Exists() && function.RefOutputVars.Value.Split(' ').Contains(var.ID.Value))
                        LocalVariables.Add(new Variable(var, Variable.Kind.Output));
                    else
                        LocalVariables.Add(new Variable(var, Variable.Kind.Local));
                }
            }
            // Testing incoming data for correctness
            Func<Variable, bool> inputVariables = delegate(Variable v) { return v.kind == Variable.Kind.Input; };
            Func<Variable, bool> outputVariables = delegate(Variable v) { return v.kind == Variable.Kind.Output; };
            int input_vars = LocalVariables.Count(inputVariables);
            int output_vars = LocalVariables.Count(outputVariables);
            if (function.RefInputVars.Exists() && (function.RefInputVars.Value.Split(' ').Count() != input_vars))
                throw new ValidatorException("Referenced input variables were not found in function " + function.ID.Value);
            if (function.RefOutputVars.Exists() && (function.RefOutputVars.Value.Split(' ').Count() != output_vars))
                throw new ValidatorException("Referenced output variables were not found in function " + function.ID.Value);

            // Getting basic blocks
            foreach (BasicBlockType bb in function.BasicBlock)
                BasicBlocks.Add(new BasicBlock(bb, this));
        }

        // Methods
        public BasicBlock GetLastBasicBlock()
        {
            // It works, because we always have one single "fake end block"
            foreach (BasicBlock bb in BasicBlocks)
                if (bb.getSuccessors.Count.Equals(0))
                    return bb;
            return null;
        }
    }

    
    public partial class BasicBlock: IValidate
    {
        private IDManager _ID;
        public string ID
        {
            get { return _ID.ToString(); }
        } 
        public Function parent { get; private set; }

        private List<string> RefPredecessors = new List<string>();
        private List<string> RefSuccessors = new List<string>();

        private List<BasicBlock> Predecessors = new List<BasicBlock>();
        public List<BasicBlock> getPredecessors
        {
            get
            {
                Validate();
                return Predecessors;
            }
        }
        private List<BasicBlock> Successors = new List<BasicBlock>();
        public List<BasicBlock> getSuccessors
        {
            get
            {
                Validate();
                return Successors;
            }
        }

        public List<Instruction> Instructions = new List<Instruction>();

        public BasicBlock(BasicBlockType bb, Function func)
        {
            _ID = new IDManager(bb.ID.Value);
            parent = func;
            if (bb.Predecessors.Exists())
                foreach (string pid in bb.Predecessors.Value.Split(' '))
                    RefPredecessors.Add(pid);
            if (bb.Successors.Exists())
                foreach (string sid in bb.Successors.Value.Split(' '))
                    RefSuccessors.Add(sid);
            // Adding instructions to basic block
            foreach (InstructionType instr in bb.Instruction)
            {
                Instructions.Add(new Instruction(instr, this));
            }
        }

        public override bool Equals(object obj)
        {
            return (obj as BasicBlock) == null ? base.Equals(obj) : ((BasicBlock)obj).ID == ID;
        }

        /// <summary>
        /// Function to find out whether a basic block is in a loop body, or not.
        /// </summary>
        /// <returns>True if the basic block is in a loop, False if not.</returns>
        public bool isLoopBody()
        {
            foreach (BasicBlock bb in Successors)
            {
                if(bb.Equals(this)) return true;
            }

            return false;
        }
    }
    

    public partial class Variable : IValidate
    {
        // Enumerations
        public enum Kind
        {
            Input = 0,
            Output = 1,
            Local = 2,
            Global = 3
        }
        public enum State
        {
            Free = 0,
            Filled = 1,
            Used = 2,
            Not_Initialized = 3
        }
        // Attributes
        private IDManager _ID;
        public string ID
        {
            get { return _ID.ToString(); }
        }
        public ExchangeFormat.SizeType.EnumValues size { get; private set; }
        public bool pointer { get; private set; }
        public string name { get; private set; }
        public string fixedValue { get; private set; }
        public string globalID { get; private set; }
        public bool fake { get; private set; }
        public Kind kind { get; private set; }

        // Constructor
        public Variable(VariableType var, Kind kind1)
        {
            _ID = new IDManager(var.ID.Value);
            name = var.Value;
            size = var.Size.EnumerationValue;
            pointer = var.Pointer.Value;
            fixedValue = var.FixedValue.Exists() ? var.FixedValue.Value : string.Empty;
            globalID = var.GlobalID.Exists() ? var.GlobalID.Value : string.Empty;
            fake = var.Fake.Exists() ? var.Fake.Value : false;
            kind = kind1;
        }
    }


    public partial class Instruction : IComparable, IValidate
    {
        public BasicBlock parent { get; private set; }
        private IDManager _ID;
        public string ID
        {
            get { return _ID.ToString(); }
        }
        public ExchangeFormat.StatementTypeType.EnumValues statementType { get; private set; }
        public string text { get; private set; }
        public bool polyRequired { get; private set; }

        public List<Variable> RefVariables = new List<Variable>();
        public Dictionary<Variable, Variable.State> DeadVariables = new Dictionary<Variable, Variable.State>();

        public Instruction(InstructionType instr, BasicBlock par)
        {
            parent = par;
            _ID = new IDManager(instr.ID.Value);
            statementType = instr.StatementType.EnumerationValue;
            text = instr.Value;
            polyRequired = instr.PolyRequired.Exists() ? instr.PolyRequired.Value : false;
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

        public override bool Equals(object obj)
        {
            return (obj as BasicBlock) == null ? base.Equals(obj) : ((BasicBlock)obj).ID == ID;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Gets a list of the instructions followed by this instruction
        /// </summary>
        /// <returns>A list of preceding instructions (or empty list if no such found)</returns>
        public List<Instruction> GetPrecedingInstructions()
        {
            List<Instruction> preceding = new List<Instruction>();
            int number = parent.Instructions.BinarySearch(this);
            if (number > 0)
            {
                preceding.Add(parent.Instructions[number - 1]);
                return preceding;
            }
            else if (number == 0)
            {
                foreach (BasicBlock bb in parent.getPredecessors)
                {
                    preceding.Add(bb.Instructions[bb.Instructions.Count - 1]);
                }
                return preceding;
            }
            else return null;
        }

        /// <summary>
        /// Gets a list of the instructions following this instruction
        /// </summary>
        /// <returns>A list of following instructions (or empty list if no such found)</returns>
        public List<Instruction> GetNextInstructions()
        {
            List<Instruction> next = new List<Instruction>();
            int number = parent.Instructions.BinarySearch(this);
            if (number < parent.Instructions.Count - 1)
            {
                next.Add(parent.Instructions[number + 1]);
                return next;
            }
            else if (number == parent.Instructions.Count - 1)
            {
                foreach (BasicBlock bb in parent.getSuccessors)
                {
                    next.Add(bb.Instructions[0]);
                }
                return next;
            }
            else return null;
        }
    }


    public class IDManager
    {
        private string ID;
        private const string startID = "ID_";
        public IDManager()
        {
            ID = string.Concat(startID, Guid.NewGuid().ToString()).ToUpper();
        }
        public IDManager(string id)
        {
            ID = id;
        }
        public void setAndCheckID(string id)
        {
            if (Regex.IsMatch(id, @"^ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None))
                ID = id;
            else
                throw new ArgumentException("Incorrect ID. The unique identifier " + id + " is not in a form ID_'GUID'");
        }

        public override bool Equals(object obj)
        {
            return (obj as IDManager) == null ? base.Equals(obj) : ((IDManager)obj).ID == ID;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return ID;
        }
    }
}
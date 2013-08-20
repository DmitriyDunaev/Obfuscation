using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExchangeFormat;
using System.Text.RegularExpressions;
using Obfuscator;

namespace Internal
{
    /// <summary>
    /// Class to store the state and the pointed variable of a pointer.
    /// </summary>
    [Serializable]
    public class PointerData
    {
        // Attributes

        /// <summary>
        /// The pointed variable.
        /// </summary>
        public Variable PointsTo { get; set; }

        /// <summary>
        /// The state.
        /// </summary>
        public Variable.State State { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="state">The state of the pointer.</param>
        /// <param name="var">The variable it points to.</param>
        public PointerData(Variable.State state, Variable var)
        {
            State = state;
            PointsTo = var;
        }
    }


    [Serializable]
    public partial class Routine : IValidate
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


    [Serializable]
    public partial class Function : IValidate
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
    }


    [Serializable]
    public partial class BasicBlock : IValidate
    {
        //Attributes
        private IDManager _ID;
        public string ID
        {
            get { return _ID.ToString(); }
        }
        public Function parent { get; private set; }
        public bool dead = false;


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

        //Constructors
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
                Instructions.Add(new Instruction(instr, this));
        }



        /// <summary>
        /// Constructor for BasicBlock that contains one NoOperation instruction
        /// </summary>
        /// <param name="parent">The parent function, which will contain the block</param>
        public BasicBlock(Function parent)
        {
            if (parent == null || parent.parent == null)
                throw new ObfuscatorException("Basic block is created outside Function.");
            Instruction instruction = new Instruction(StatementTypeType.EnumValues.eNoOperation, this);
            Instructions.Add(instruction);
            _ID = new IDManager();
            this.parent = parent;
            parent.BasicBlocks.Add(this);
        }

        // Overloaded methods
        public override bool Equals(object obj)
        {
            return (obj as BasicBlock) == null ? base.Equals(obj) : ((BasicBlock)obj).ID == ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }


    [Serializable]
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
        public enum Purpose
        {
            Original = 1,
            Temporary = 2,
            Fake = 3,
            ConstRecalculation = 4
        }

        // Attributes
        private IDManager _ID;
        public string ID
        {
            get { return _ID.ToString(); }
        }
        public int memoryUnitSize { get; private set; }
        public bool pointer { get; private set; }
        public string name { get; private set; }
        public int memoryRegionSize { get; private set; }
        public string fixedValue { get; private set; }
        public string globalID { get; private set; }
        public int? fixedMin { get; set; }
        public int? fixedMax { get; set; }
        public bool fake { get; private set; }
        public Kind kind { get; private set; }

        // Constructor
        public Variable(VariableType var, Kind kind1)
        {

            _ID = new IDManager(var.ID.Value);
            name = var.Value;
            memoryRegionSize = Convert.ToInt32(var.MemoryRegionSize.Value);
            memoryUnitSize = var.MemoryUnitSize.Exists() ? Convert.ToInt32(var.MemoryUnitSize.Value) : 1;
            pointer = var.Pointer.Value;
            fixedValue = var.FixedValue.Exists() ? var.FixedValue.Value : string.Empty;
            globalID = var.GlobalID.Exists() ? var.GlobalID.Value : string.Empty;
            fake = var.Fake.Exists() ? var.Fake.Value : false;
            kind = kind1;
            if (var.MinValue.Exists())
                fixedMin = Convert.ToInt32(var.MinValue.Value);
            else
                fixedMin = null;
            if (var.MaxValue.Exists())
                fixedMax = Convert.ToInt32(var.MaxValue.Value);
            else
                fixedMax = null;
        }

        public Variable(Kind kind, Purpose purpose, int memory_region_size = 4, int? min_value = null, int? max_value = null)
        {
            _ID = new IDManager();
            switch (purpose)
            {
                case Purpose.Original:
                    name = string.Concat("v_", ID);
                    break;
                case Purpose.Temporary:
                    name = string.Concat("t_", ID);
                    break;
                case Purpose.Fake:
                    name = string.Concat("f_", ID);
                    break;
                case Purpose.ConstRecalculation:
                    name = string.Concat("c_", ID);
                    break;
                default:
                    throw new ObfuscatorException("Unsupported Variable.Purpose value.");
            }
            memoryRegionSize = memory_region_size;
            pointer = false;
            fake = true;
            fixedMin = min_value;
            fixedMax = max_value;
            this.kind = kind;
        }


        public override bool Equals(object obj)
        {
            return (obj as Variable) == null ? base.Equals(obj) : ((Variable)obj).ID == ID;
        }


        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }


    [Serializable]
    public partial class Instruction : IComparable, IValidate
    {
        public enum ArithmeticOperationType
        {
            Addition = 0,
            Subtraction = 1,
            Multiplication = 2,
            Division = 3
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
        public StatementTypeType.EnumValues statementType { get; private set; }
        public string TACtext { get; set; }
        public bool polyRequired { get; internal set; }
        public bool isFake { get; private set; }

        public List<Variable> RefVariables = new List<Variable>();
        public Dictionary<Variable, Variable.State> DeadVariables = new Dictionary<Variable, Variable.State>();

        /// <summary>
        /// It stores the state, and the pointed variable of the pointer (key).
        /// </summary>
        public Dictionary<Variable, PointerData> DeadPointers = new Dictionary<Variable, PointerData>();

        //Constructors
        public Instruction(InstructionType instr, BasicBlock par)
        {
            parent = par;
            _ID = new IDManager(instr.ID.Value);
            statementType = instr.StatementType.EnumerationValue;
            TACtext = instr.Value;
            isFake = false;
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

        /// <summary>
        /// Copy constructor with adjustable parent
        /// </summary>
        /// <param name="ins">The instruction to copy</param>
        /// <param name="par">The new parent of the instuction. By default, it is the parent of the instruction</param>
        public Instruction(Instruction ins, BasicBlock par = null)
        {
            if (par == null) parent = ins.parent;
            else parent = par;
            _ID = new IDManager();
            statementType = ins.statementType;
            TACtext = ins.TACtext;
            isFake = false;
            polyRequired = false;
            RefVariables = ins.RefVariables;
            DeadPointers = ins.DeadPointers;
            DeadVariables = ins.DeadVariables;
        }

        private void setInstructionValues(BasicBlock parent, StatementTypeType.EnumValues statementType, string TACtext, List<Variable> refVariables, bool polyRequired = false)
        {
            this._ID = new IDManager();
            this.isFake = true;
            this.parent = parent;
            this.statementType = statementType;
            this.TACtext = TACtext;
            this.RefVariables = refVariables == null ? new List<Variable>() : refVariables;
            this.polyRequired = polyRequired;
        }

        public Instruction(StatementTypeType.EnumValues statementType, BasicBlock parent = null)
        {
            switch (statementType)
            {
                case StatementTypeType.EnumValues.eNoOperation:
                    setInstructionValues(parent, statementType, "nop", null);
                    break;
                default:
                    throw new ObfuscatorException("Only the 'NoOperation' type instruction can be created.\n");
            }
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
    }


    [Serializable]
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
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return ID;
        }
    }
}
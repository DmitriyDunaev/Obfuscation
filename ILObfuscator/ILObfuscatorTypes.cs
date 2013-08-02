using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExchangeFormat;

namespace Obfuscator
{
    public class Routine
    {
        // Attributes
        private string description;
        public List<Function> Functions = new List<Function>();
        // Constructor
        public Routine(Exchange doc)
        {
            description = doc.Routine[0].Description.Value;
            foreach (FunctionType function in doc.Routine[0].Function)
                Functions.Add(new Function(function, this));
        }
    }

    public class Function
    {
        // Attributes
        public Routine parent;
        private IDManager ID = new IDManager();
        public string getID()
        {
            return ID.ToString();
        }
        public CalledFromType.EnumValues calledFrom;
        private string externalLabel;
        public List<Variable> Variables = new List<Variable>();
        public List<BasicBlock> BasicBlocks = new List<BasicBlock>();
        // Constructor
        public Function(FunctionType function, Routine par)
        {
            ID = new IDManager(function.ID.Value);
            parent = par;
            calledFrom = function.CalledFrom.EnumerationValue;
            externalLabel = function.ExternalLabel.Value;
            // Collecting all variables
            if (function.Inputs.Exists && function.Inputs[0].Original.Exists && function.Inputs[0].Original[0].Variable.Exists)
                foreach (VariableType var in function.Inputs[0].Original[0].Variable)
                    Variables.Add(new Variable(var, Variable.Kind_IO.Input, Variable.Kind_OF.Original));
            if (function.Inputs.Exists && function.Inputs[0].Fake.Exists && function.Inputs[0].Fake[0].Variable.Exists)
                foreach (VariableType var in function.Inputs[0].Fake[0].Variable)
                    Variables.Add(new Variable(var, Variable.Kind_IO.Input, Variable.Kind_OF.Fake));
            if (function.Outputs.Exists && function.Outputs[0].Original.Exists && function.Outputs[0].Original[0].Variable.Exists)
                foreach (VariableType var in function.Outputs[0].Original[0].Variable)
                    Variables.Add(new Variable(var, Variable.Kind_IO.Output, Variable.Kind_OF.Original));
            if (function.Outputs.Exists && function.Outputs[0].Fake.Exists && function.Outputs[0].Fake[0].Variable.Exists)
                foreach (VariableType var in function.Outputs[0].Fake[0].Variable)
                    Variables.Add(new Variable(var, Variable.Kind_IO.Output, Variable.Kind_OF.Fake));
            if (function.Locals.Exists && function.Locals[0].Original.Exists && function.Locals[0].Original[0].Variable.Exists)
                foreach (VariableType var in function.Locals[0].Original[0].Variable)
                    Variables.Add(new Variable(var, Variable.Kind_IO.Local, Variable.Kind_OF.Original));
            if (function.Locals.Exists && function.Locals[0].Fake.Exists && function.Locals[0].Fake[0].Variable.Exists)
                foreach (VariableType var in function.Locals[0].Fake[0].Variable)
                    Variables.Add(new Variable(var, Variable.Kind_IO.Local, Variable.Kind_OF.Fake));

            foreach (BasicBlockType bb in function.BasicBlock)
                BasicBlocks.Add(new BasicBlock(bb, this));
        }
        // Methods
        public BasicBlock GetLastBasicBlock()
        {
            // It works, because we always have one single "fake end block"
            foreach (BasicBlock bb in BasicBlocks)
                if (bb.getAllSuccessors().Count.Equals(0))
                    return bb;
            return null;
        }
    }


    public class Variable
    {
        // Enumerations
        public enum Kind_IO
        {
            Input = 0,
            Output = 1,
            Local = 2
        }
        public enum Kind_OF
        {
            Original = 0,
            Fake = 1
        }
        public enum State
        {
            Free = 0,
            Filled = 1,
            Used = 2,
            Not_Initialized = 3
        }
        // Attributes
        private IDManager ID = new IDManager();
        public string getID()
        {
            return ID.ToString();
        }
        public string name;
        public List<int> constValueInParam;
        public ExchangeFormat.SizeType.EnumValues size;
        public bool pointer;
        public Kind_IO kind_io;
        public Kind_OF kind_of;
        // Constructor
        public Variable(VariableType var, Kind_IO kind_io1, Kind_OF kind_of1)
        {
            ID = new IDManager(var.ID.Value);
            name = var.Value;
            if (var.ConstValueInParam.Exists())
            {
                constValueInParam = new List<int>(1);
                constValueInParam.Add(var.ConstValueInParam.Value);
            }
            size = var.Size.EnumerationValue;
            pointer = var.Pointer.Value;
            kind_io = kind_io1;
            kind_of = kind_of1;
        }
    }


    public class BasicBlock
    {
        private IDManager ID = new IDManager();
        public string getID()
        {
            return ID.ToString();
        }
        public Function parent;

        private List<BasicBlock> Predecessors = new List<BasicBlock>();
        private List<BasicBlock> Successors = new List<BasicBlock>();
        private List<IDManager> RefPredecessors = new List<IDManager>();
        private List<IDManager> RefSuccessors = new List<IDManager>();

        public List<BasicBlock> getAllPredeccessors()
        {
            if (RefPredecessors.Count.Equals(0))
                return Predecessors;
            foreach (IDManager id in RefPredecessors)
                foreach (BasicBlock bb in parent.BasicBlocks)
                    if (bb.ID.Equals(id))
                        Predecessors.Add(bb);
            if (RefPredecessors.Count.Equals(Predecessors.Count))
            {
                RefPredecessors.Clear();
                return Predecessors;
            }
            else
                throw new Exception("Referenced basic block was not found. Referenced block:" + RefPredecessors[0].ToString());
        }

        public List<BasicBlock> getAllSuccessors()
        {
            if (RefSuccessors.Count.Equals(0))
                return Successors;
            foreach (IDManager id in RefSuccessors)
                foreach (BasicBlock bb in parent.BasicBlocks)
                    if (bb.ID.Equals(id))
                        Successors.Add(bb);
            if (RefSuccessors.Count.Equals(Successors.Count))
            {
                RefSuccessors.Clear();
                return Successors;
            }
            else
                throw new Exception("Referenced basic block was not found. Referenced block:" + RefSuccessors[0].ToString());
        }

        public List<Instruction> Instructions = new List<Instruction>();

        public BasicBlock(BasicBlockType bb, Function func)
        {
            ID = new IDManager(bb.ID.Value);
            parent = func;
            if (bb.Predecessors.Exists())
                foreach (string pid in bb.Predecessors.Value.Split(' '))
                    RefPredecessors.Add(new IDManager(pid));
            if (bb.Successors.Exists())
                foreach (string sid in bb.Successors.Value.Split(' '))
                    RefSuccessors.Add(new IDManager(sid));
            // Adding instructions to basic block
            foreach (InstructionType instr in bb.Instruction)
            {
                Instructions.Add(new Instruction(instr, this));
            }
        }
    }


    public class Instruction : IComparable
    {
        public BasicBlock parent;
        private IDManager ID = new IDManager();
        public string getID()
        {
            return ID.ToString();
        }
        public ExchangeFormat.StatementTypeType.EnumValues statementType;
        public string text;
        public bool polyRequired;
        public List<Variable> RefVariables = new List<Variable>();
        public List<Variable> DeadVariables = new List<Variable>();
        public Dictionary<Variable, Variable.State> DeadVariables2 = new Dictionary<Variable, Variable.State>();

        public Instruction(InstructionType instr, BasicBlock par)
        {
            parent = par;
            ID = new IDManager(instr.ID.Value);
            statementType = instr.StatementType.EnumerationValue;
            text = instr.Value;
            if (instr.PolyRequired.Exists())
                polyRequired = instr.PolyRequired.Value;
            if (instr.RefVars.Exists())
            {
                foreach (string vid in instr.RefVars.Value.Split(' '))
                {
                    foreach (Variable var in parent.parent.Variables)
                    {
                        if (var.getID().Equals(vid))
                            RefVariables.Add(var);
                    }
                }
                if (!instr.RefVars.Value.Split(' ').Length.Equals(RefVariables.Count))
                    throw new Exception("Referenced variable was not found. Instruction: " + instr.ID.Value);
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
            throw new ArgumentException("Comparison error! Instruction " + (obj as Instruction).ID.ToString() + " is not found in basic block.");
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            return (obj as Instruction).ID.Equals(ID);
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
        public void setAndCheckID(string ID)
        {
            if (!ID.ToUpper().StartsWith(startID))
                throw new FormatException("ID has inappropriate format: it should start with " + startID + " followed by a GUID.");
            Guid value = new Guid(ID.Substring(3));
            ID = string.Concat(startID, value.ToString()).ToUpper();
        }

        public override bool Equals(object obj)
        {
            return (((IDManager)obj).ID == ID);
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

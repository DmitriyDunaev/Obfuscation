using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExchangeFormat;


namespace Objects
{
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

        public Common.CalledFrom calledFrom { get; private set; }

        public List<Variable> LocalVariables = new List<Variable>();
        public List<BasicBlock> BasicBlocks = new List<BasicBlock>();

        public bool containsDivisionModulo = false;
        public bool irreducibleCFG = false;

        // Constructor
        public Function(FunctionType function, Routine par)
        {
            _ID = new IDManager(function.ID.Value);
            parent = par;
            calledFrom = (Common.CalledFrom)function.CalledFrom.EnumerationValue;
            globalID = function.GlobalID.Value;
            // Collecting all variables
            if (function.Local.Exists)
            {
                foreach (VariableType var in function.Local.First.Variable)
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
            if (function.RefInputVars.Exists() && (function.RefInputVars.Value.Split(' ').Count() != LocalVariables.Count(x => x.kind == Variable.Kind.Input)))
                throw new ValidatorException("Some referenced input variables were not found in function " + function.ID.Value);
            if (function.RefOutputVars.Exists() && (function.RefOutputVars.Value.Split(' ').Count() != LocalVariables.Count(x => x.kind == Variable.Kind.Output)))
                throw new ValidatorException("Some referenced output variables were not found in function " + function.ID.Value);

            // Getting basic blocks
            foreach (BasicBlockType bb in function.BasicBlock)
                BasicBlocks.Add(new BasicBlock(bb, this));
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ID"></param>
        public Function(Routine routine, String ID)
        {
            _ID = new IDManager(ID);
            globalID = ID;
            parent = routine;
            calledFrom = Common.CalledFrom.InternalOnly;
        }


        // METHODS


        /// <summary>
        /// Gets the "fake exit block" (it is not the last block in the list) of a function
        /// </summary>
        /// <returns>The "fake exit block"</returns>
        public BasicBlock GetFakeExitBasicBlock()
        {
            return BasicBlocks.Find(x => x.getSuccessors.Count == 0);
        }

        /// <summary>
        /// Gets the "fake exit block" (it is not the last block in the list) of a function
        /// </summary>
        /// <returns>The "fake exit block"</returns>
        public BasicBlock GetLastBasicBlock()
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
                throw new ObjectException("Wrong parameter passing: minvalue cannot exceed maxvalue.");
            if (!min_value.HasValue && !max_value.HasValue)
                throw new ObjectException("One of values (FixedMin or FixedMax) should be a not-null value.");
            if ((min_value.HasValue && min_value.Value < 0) || (max_value.HasValue && max_value.Value < 0))
                throw new ObjectException("At present time only positive integer FixedMin and FixedMax are supported.");
            Variable fake_input = new Variable(Variable.Kind.Input, Variable.Purpose.Fake, Common.MemoryRegionSize.Integer, min_value, max_value);
            LocalVariables.Add(fake_input);
            return fake_input;
        }
        

        /// <summary>
        /// Checks whether this functions contains either "division" or "modulo" operations
        /// If they do, the output should be changed only right before the return to avoid problems
        /// with the operations
        /// </summary>
        public void CheckDivisionModulo()
        {
            foreach (BasicBlock bb in this.BasicBlocks)
            {
                List<Instruction> divModInstructions = bb.Instructions.FindAll(x => x.TACtext.Contains("%") || x.TACtext.Contains("/"));
                if (divModInstructions.Count > 0)
                {
                    this.containsDivisionModulo = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Add all the variables inside of the funtion to the LocalVariables list.
        /// </summary>
        public void AddVariables()
        {
            HashSet<String> used = new HashSet<String>();
            foreach (BasicBlock bb in this.BasicBlocks)
            {
                foreach (Instruction ins in bb.Instructions)
                {
                    foreach (Variable var in ins.RefVariables)
                    {
                        if (used.Add(var.ID))
                        {
                            if (!var.fixedMin.HasValue && !var.fixedMax.HasValue)
                            {
                                var.fixedMin = 0; //TODO validation
                                var.fixedMax = 100000; //TODO validation
                            }
                            var.kind = Variable.Kind.Input; //TODO
                            this.LocalVariables.Add(var);
                        }
                    }
                }
            }
        }
        
    }
}

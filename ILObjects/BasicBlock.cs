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
    public partial class BasicBlock : IValidate
    {
        // Enumerations
        public enum InvolveInFakeCodeGeneration
        {
            FakeVariablesOnly = 0,
            OriginalVariablesOnly = 1,
            Both = 2
        }

        //Attributes
        private IDManager _ID;
        public string ID
        {
            get { return _ID.ToString(); }
        }
        public Function parent { get; private set; }
        public bool inFakeLane = false;
        public InvolveInFakeCodeGeneration Involve = InvolveInFakeCodeGeneration.FakeVariablesOnly;
        public bool Meshable = true;
        public bool PolyRequired = false;

        private List<string> RefPredecessors = new List<string>();
        private List<string> RefSuccessors = new List<string>();

        private List<BasicBlock> Predecessors = new List<BasicBlock>();
        public List<BasicBlock> getPredecessors
        {
            get
            {
                return Predecessors;
            }
        }
        private List<BasicBlock> Successors = new List<BasicBlock>();
        public List<BasicBlock> getSuccessors
        {
            get
            {
                return Successors;
            }
        }
        public List<Instruction> Instructions = new List<Instruction>();

        //Constructors
        public BasicBlock(BasicBlockType bb, Function func)
        {
            _ID = new IDManager(bb.ID.Value);
            parent = func;
            PolyRequired = bb.PolyRequired.Exists() ? bb.PolyRequired.Value : false;
            if (bb.Predecessors.Exists())
                foreach (string pid in bb.Predecessors.Value.Split(' '))
                    RefPredecessors.Add(pid);
            if (bb.Successors.Exists())
                foreach (string sid in bb.Successors.Value.Split(' '))
                    RefSuccessors.Add(sid);
            // Adding instructions to basic block
            foreach (InstructionType instr in bb.Instruction)
                Instructions.Add(new Instruction(instr, this));

            // Trying to link real successors to this basic block
            switch (RefSuccessors.Count)
            {
                case 1:
                    if (parent.BasicBlocks.ConvertAll(y => y.ID).Contains(RefSuccessors[0]))
                    {
                        Successors.Add(parent.BasicBlocks.Find(y => y.ID == RefSuccessors[0]));
                        RefSuccessors.Clear();
                    }
                    break;
                case 2:
                    if (parent.BasicBlocks.ConvertAll(y => y.ID).Contains(RefSuccessors[0]))
                        Successors.Insert(0, parent.BasicBlocks.Find(y => y.ID == RefSuccessors[0]));
                    else if (parent.BasicBlocks.ConvertAll(y => y.ID).Contains(RefSuccessors[1]))
                        Successors.Add(parent.BasicBlocks.Find(y => y.ID == RefSuccessors[1]));
                    if (Successors.Count == 2)
                        RefSuccessors.Clear();
                    break;
                default:
                    break;
            }

            // Trying to link real predecessors to this basic block
            if (RefPredecessors.Count() > 0)
                RefPredecessors.ForEach(x => { parent.BasicBlocks.ForEach(y => { if (y.ID == x) Predecessors.Add(y); }); });
            Predecessors.ForEach(x => RefPredecessors.Remove(x.ID));

            // Trying to link this (new) basic block as predecessor/successor to other basic blocks of the function
            foreach (BasicBlock block in parent.BasicBlocks)
            {
                if (block.RefPredecessors.Contains(ID))
                {
                    block.Predecessors.Add(this);
                    block.RefPredecessors.Remove(ID);
                }

                if (block.RefSuccessors.Contains(ID))
                    switch (block.RefSuccessors.Count)
                    {
                        case 1:
                            block.Successors.Add(this);
                            block.RefSuccessors.Clear();
                            break;
                        case 2:
                            if (block.RefSuccessors[0] == ID)
                                block.Successors.Insert(0, this);
                            else
                                block.Successors.Add(this);
                            if (block.Successors.Count == 2)
                                block.RefSuccessors.Clear();
                            break;
                        default:
                            break;
                    }
            }
        }

        /// <summary>
        /// Constructor for BasicBlock that contains one NoOperation instruction
        /// </summary>
        /// <param name="parent">The parent function, which will contain the block</param>
        public BasicBlock(Function parent)
        {
            if (parent == null || parent.parent == null)
                throw new ObjectException("Basic block cannot be created outside a function.");
            Instruction instruction = new Instruction(this);
            Instructions.Add(instruction);
            _ID = new IDManager();
            this.parent = parent;
            parent.BasicBlocks.Add(this);
        }

        /// <summary>
        /// Basic constructor for other methods.
        /// </summary>
        private BasicBlock() { }

        /// <summary>
        /// Creates a new Basic Block with the given function as parent, and with the ID if given.
        /// </summary>
        /// <param name="function"></param>
        /// <param name="ID"></param>
        /// <returns>The new Basic Block.</returns>
        public static BasicBlock getBasicBlock(Function function, string ID = "")
        {
            if (function == null || function.parent == null)
                throw new ObjectException("Basic block cannot be created outside a routine.");
            BasicBlock block = new BasicBlock();
            block._ID = !String.IsNullOrEmpty(ID) ? new IDManager(ID) : new IDManager();
            block.parent = function;
            function.BasicBlocks.Add(block);
            return block;
        }

        /// <summary>
        /// Copy constructor for basic block
        /// </summary>
        /// <param name="original">Original basic block to be copied</param>
        /// <param name="newSuccessors">List of new successors</param>
        public BasicBlock(BasicBlock original, List<BasicBlock> newSuccessors)
        {
            _ID = new IDManager();
            this.inFakeLane = original.inFakeLane;
            this.parent = original.parent;
            this.Involve = original.Involve;
            this.Meshable = original.Meshable;
            this.PolyRequired = true;
            Instructions = Common.DeepClone(original.Instructions) as List<Instruction>;
            Instructions.ForEach(x => x.ResetID());
            Instructions.ForEach(x => x.parent = this);
            newSuccessors.ForEach(x => LinkToSuccessor(x));

            if (Successors.Count() == 1 && (Instructions.Last().statementType != Common.StatementType.UnconditionalJump && Instructions.Last().statementType != Common.StatementType.Procedural))
            {
                Instructions.Add(new Instruction(this));

                Instructions.Last().statementType = Common.StatementType.UnconditionalJump;
                Instructions.Last().TACtext = string.Join(" ", "goto", Successors.First().ID);
                Instructions.Last().parent.LinkToSuccessor(Successors.First(), true);

            }

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


        // METHODS

        public Boolean isFakeExitBlock()
        {
            return Successors.Count == 0 && Predecessors.Count > 0 && Instructions.Count == 1 && Instructions[0].TACtext == "return";
        }


        /// <summary>
        /// Splits the basic block into two after the given instruction (+ handles successor-predecessor links)
        /// </summary>
        /// <param name="inst">Instruction to split after</param>
        /// <returns>The new basic block (the second after splitting)</returns>
        public BasicBlock SplitAfterInstruction(Instruction inst)
        {
            if (!inst.parent.Equals(this) || !this.Instructions.Contains(inst))
                throw new ObjectException("The instruction does not belong to the given basic block.");
            if (inst.statementType == Common.StatementType.ConditionalJump)
                throw new ObjectException("You cannot split a basic block after the conditional jump.");
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
            if (inst.statementType == Common.StatementType.UnconditionalJump)
                Instructions.Last().TACtext = Instructions.Last().TACtext.Replace(Regex.Match(Instructions.Last().TACtext, @"\bID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b").Value, newBB.ID);
            return newBB;
        }

        /// <summary>
        /// Links the basic block to a new successor (sets this.Successor and successor.Predecessor properties)
        /// </summary>
        /// <param name="successor">A new successor basic block</param>
        /// <param name="clear">If true - clears all other successor-predecessor links of the basic block before linking to a new successor</param>
        /// /// <param name="clear">If true - the successor is linked as 'true' (important for conditional jumps)</param>
        public void LinkToSuccessor(BasicBlock successor, bool clear = false, bool true_branch = false)
        {
            if (Successors.Count > 2)
                throw new ObjectException("Basic block cannot have more than 2 successors.");

            if (clear)
            {
                foreach (BasicBlock next in Successors)
                    next.Predecessors.Remove(this);
                Successors.Clear();
            }

            if (true_branch)
                Successors.Insert(0, successor);
            else
                Successors.Add(successor);

            successor.Predecessors.Add(this);
        }
    }
}

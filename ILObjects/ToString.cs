using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Objects
{
    public partial class Routine
    {
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("********************************************************************");
            sb.AppendLine("ROUTINE:");
            sb.AppendLine("- Description: " + description);
            sb.AppendLine("- Functions: " + Functions.Count);
            sb.AppendLine("- Global variables: " + GlobalVariables.Count);
            sb.AppendLine("    - Original:" + GlobalVariables.Count(x => !x.fake));
            sb.AppendLine("    - Fake:" + GlobalVariables.Count(x => x.fake));
            sb.AppendLine("********************************************************************");
            //sb.AppendLine("GLOBAL VARIABLES:");
            //foreach (Variable var in GlobalVariables)
            //{
            //    sb.AppendLine(var.ToString());
            //}
            //sb.AppendLine("********************************************************************");
            sb.AppendLine("FUNCTIONS:");
            foreach (Function func in Functions)
                sb.Append(func.ToString());
            return sb.ToString();
        }
    }


    public partial class Function
    {
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("********************************************************************");
            sb.AppendLine("FUNCTION ID: " + ID);
            sb.AppendLine("- Global ID: " + globalID);
            sb.AppendLine("- Called from: " + calledFrom);
            sb.AppendLine("- Basic blocks total: " + BasicBlocks.Count);
            sb.AppendLine("    - Dead basic blocks: " + BasicBlocks.Count(x => x.inFakeLane));
            sb.AppendLine("- Local variables total: " + LocalVariables.Count(x => x.kind != Variable.Kind.Global));
            sb.AppendLine("    - Original input parameters: " + LocalVariables.Count(x => x.kind == Variable.Kind.Input && !x.fake));
            sb.AppendLine("    - Original output parameters: " + LocalVariables.Count(x => x.kind == Variable.Kind.Output && !x.fake));
            sb.AppendLine("    - Original variables: " + LocalVariables.Count(x => x.kind == Variable.Kind.Local && !x.fake));
            sb.AppendLine("    - Fake input parameters: " + LocalVariables.Count(x => x.kind == Variable.Kind.Input && x.fake));
            sb.AppendLine("    - Fake output parameters: " + LocalVariables.Count(x => x.kind == Variable.Kind.Output && x.fake));
            sb.AppendLine("    - Fake variables: " + LocalVariables.Count(x => x.kind == Variable.Kind.Local && x.fake));
            sb.AppendLine("********************************************************************");
            sb.AppendLine("BASIC BLOCKS:");
            foreach (BasicBlock bb in BasicBlocks)
                sb.AppendLine(bb.ToString());
            return sb.ToString();
        }
    }


    public partial class BasicBlock
    {
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("********************************");
            sb.AppendLine("Basic Block ID: " + ID);
            sb.AppendLine("- Parent: " + parent.ID);
            sb.AppendLine("- Dead: " + inFakeLane.ToString().ToLower());
            sb.AppendLine("- Predecessors total: " + Predecessors.Count);
            foreach (BasicBlock pred in Predecessors)
                sb.AppendLine("     - Predecessor ID: " + pred.ID);
            sb.AppendLine("- Successors total: " + Successors.Count);
            foreach (BasicBlock succ in Successors)
                sb.AppendLine("     - Successor ID: " + succ.ID);
            sb.AppendLine("- Instructions total: " + Instructions.Count);
            sb.AppendLine("    - Original: " + Instructions.Count(x => !x.isFake));
            sb.AppendLine("    - Fake: " + Instructions.Count(x => x.isFake));
            sb.AppendLine("****************");
            foreach (Instruction inst in Instructions)
                sb.AppendLine(inst.ToString());
            return sb.ToString();
        }
    }



    public partial class Instruction
    {
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(statementType.ToString().Substring(1));
            for (int i = 0; i < (20 - statementType.ToString().Substring(1).Length); i++)
                sb.Append(" ");
            sb.Append("|\t");
            sb.Append(TACtext);
            return sb.ToString();
        }
    }
}

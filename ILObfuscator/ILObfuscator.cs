using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILObfuscator
{
    public class Routine
    {
        private string description;
        public static List<Function> functions;
    }

    public class Function
    {
        public IDManager ID = new IDManager();
        
        public List<Variable> inputs;   //?? Maybe better to store together??    
        public List<Variable> outputs;  //??
        public List<Variable> locals;   //??

        // public List<Variable> Variables;   ??????????

        //public Variable getVariableByID(string ID)
        //{
        //    foreach (Variable var in inputs)
        //    {
        //        if(string.Equals(var.getID(),ID))
        //            return var;
        //    }
        //    foreach (Variable var in outputs)
        //    {
        //        if (string.Equals(var.getID(), ID))
        //            return var;
        //    }
        //    foreach (Variable var in locals)
        //    {
        //        if (string.Equals(var.getID(), ID))
        //            return var;
        //    }
        //    return null;
        //}
    }

    public class Variable
    {
        public IDManager ID = new IDManager();
        public string name;
        public string constValueInParam;
        public Obfuscator.SizeType.EnumValues size;
        public bool pointer;
    }

    public class BasicBlock
    {
        public IDManager ID = new IDManager();
        public List<BasicBlock> predecessors;
        public List<BasicBlock> successors;
        public List<Instruction> instructions;
    }

    public class Instruction
    {
        public IDManager ID = new IDManager();

        public Obfuscator.StatementTypeType.EnumValues statementType;
        public string text;
        public bool polyRequired;
        List<Variable> refVars;
    }



    public class IDManager
    {
        private string ID;
        private const string startID = "ID_";
        public void setID() { ID = string.Concat(startID, Guid.NewGuid().ToString()).ToUpper(); }
        public void setID(string ID) 
        {
            if (!ID.ToUpper().StartsWith(startID))
                throw new FormatException("ID has inappropriate format: it should start with " + startID + " followed by a GUID.");
            Guid value = new Guid(ID.Substring(3,36));
            ID = string.Concat(startID, value.ToString()).ToUpper();
        }
        public string getID() { return ID; }
    }

}

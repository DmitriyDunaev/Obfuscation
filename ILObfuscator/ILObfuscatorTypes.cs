using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Obfuscator;

namespace ILObfuscator
{
    public class Routine
    {
        // Attributes
        private string description;
        public List<Function> functions = new List<Function>();
        // Constructor
        public Routine(Exchange doc)
        {
            description = doc.Routine[0].Description.Value;
            foreach (FunctionType function in doc.Routine[0].Function)
                functions.Add(new Function(function));
        }
    }


    public class Function
    {
        // Attributes
        public IDManager ID = new IDManager();
        public List<Variable> Variables = new List<Variable>();
        public List<BasicBlock> BasicBlocks = new List<BasicBlock>();
        // Constructor
        public Function(FunctionType function)
        {
            ID = new IDManager(function.ID.Value);
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
            //foreach (BasicBlockType bb in function.BasicBlock)
            //    BasicBlocks.Add(new BasicBlock(bb));

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
        // Attributes
        public IDManager ID = new IDManager();
        public string name;
        public List<int> constValueInParam;
        public Obfuscator.SizeType.EnumValues size;
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
        public IDManager ID = new IDManager();
        public List<BasicBlock> predecessors = new List<BasicBlock>();
        public List<BasicBlock> successors = new List<BasicBlock>();
        public List<Instruction> instructions = new List<Instruction>();
    }

    public class Instruction
    {
        public IDManager ID = new IDManager();

        public Obfuscator.StatementTypeType.EnumValues statementType;
        public string text;
        public bool polyRequired;
        List<Variable> refVars = new List<Variable>();
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
        public string getID() { return ID; }
    }

}

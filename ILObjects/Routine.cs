using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExchangeFormat;

namespace Objects
{
    [Serializable]
    public partial class Routine : IValidate
    {
        // Attributes
        public string description { get; private set; }
        public List<Variable> GlobalVariables = new List<Variable>();
        public List<Function> Functions = new List<Function>();


        // CAST
        public static explicit operator Routine(Exchange doc)
        {
            Routine r = new Routine();
            r.description = doc.Routine[0].Description.Value;
            if (doc.Routine[0].Global.Exists)
                foreach (VariableType var in doc.Routine[0].Global[0].Variable)
                    r.GlobalVariables.Add(new Variable(var, Variable.Kind.Global));
            foreach (FunctionType function in doc.Routine[0].Function)
                r.Functions.Add(new Function(function, r));
            return r;
        }

        public static explicit operator Exchange(Routine routine)
        {
            Exchange ex = Exchange.CreateDocument();
            RoutineType root = ex.Routine.Append();
            ex.SetSchemaLocation(@"Scheme\Exchange.xsd");
            root.Description.Value = routine.description;

            // Exporting global variables
            if (routine.GlobalVariables.Count > 0)
                root.Global.Append();
            foreach (Variable global_var_orig in routine.GlobalVariables)
            {
                VariableType var = root.Global.First.Variable.Append();
                FillData(var, global_var_orig);
            }

            foreach (Function func_orig in routine.Functions)
            {
                // Function header
                FunctionType func = root.Function.Append();
                FillData(func, func_orig);
            }
            return ex;
        }


        private static void FillData(FunctionType export, Function function)
        {
            export.CalledFrom.EnumerationValue = (CalledFromType.EnumValues)function.calledFrom;
            export.GlobalID.Value = function.globalID;
            export.ID.Value = function.ID;
            List<string> input_vars_id = new List<string>();
            List<string> output_vars_id = new List<string>();
            function.LocalVariables.FindAll(x => x.kind == Variable.Kind.Input).ForEach(x => input_vars_id.Add(x.ID));
            function.LocalVariables.FindAll(x => x.kind == Variable.Kind.Output).ForEach(x => output_vars_id.Add(x.ID));
            if (input_vars_id.Count > 0)
                export.RefInputVars.Value = string.Join(" ", input_vars_id.ToArray());
            if (output_vars_id.Count > 0)
                export.RefOutputVars.Value = string.Join(" ", output_vars_id.ToArray());

            // Exporting local variables
            if (function.LocalVariables.Count > 0)
                export.Local.Append();
            foreach (Variable var_orig in function.LocalVariables)
            {
                VariableType var = export.Local.First.Variable.Append();
                FillData(var, var_orig);
            }

            // Exporting basic blocks
            foreach (BasicBlock bb_orig in function.BasicBlocks)
            {
                BasicBlockType bb = export.BasicBlock.Append();
                FillData(bb, bb_orig);
            }
        }


        private static void FillData(VariableType export, Variable variable)
        {
            // Required attributes
            export.ID.Value = variable.ID;
            export.Pointer.Value = variable.pointer;
            export.MemoryRegionSize.Value = variable.memoryRegionSize;
            export.Value = variable.name;

            // Optional Attributes
            export.Fake.Value = variable.fake;
            if (!string.IsNullOrEmpty(variable.fixedValue))
                export.FixedValue.Value = variable.fixedValue;
            if (!string.IsNullOrEmpty(variable.globalID))
                export.GlobalID.Value = variable.globalID;
            if (variable.fixedMax.HasValue)
                export.MaxValue.Value = variable.fixedMax.Value;
            if (variable.fixedMin.HasValue)
                export.MinValue.Value = variable.fixedMin.Value;
            if (variable.memoryUnitSize.HasValue)
                export.MemoryUnitSize.Value = variable.memoryUnitSize.Value;
        }


        private static void FillData(BasicBlockType export, BasicBlock bb)
        {
            export.ID.Value = bb.ID;
            List<string> predecessors = new List<string>();
            List<string> successors = new List<string>();
            bb.getPredecessors.ForEach(x => predecessors.Add(x.ID));
            bb.getSuccessors.ForEach(x => successors.Add(x.ID));
            if (predecessors.Count > 0)
                export.Predecessors.Value = string.Join(" ", predecessors.ToArray());
            if (successors.Count > 0)
                export.Successors.Value = string.Join(" ", successors.ToArray());
            // TODO: continue
            if (bb.PolyRequired)
                export.PolyRequired.Value = bb.PolyRequired;
            foreach (Instruction inst_original in bb.Instructions)
            {
                InstructionType inst = export.Instruction.Append();
                FillData(inst, inst_original);
            }
        }


        private static void FillData(InstructionType export, Instruction inst)
        {
            // Required fields
            export.ID.Value = inst.ID;
            export.StatementType.EnumerationValue = (StatementTypeType.EnumValues)inst.statementType;
            export.Value = inst.TACtext;

            // Optional values
            List<string> refvars = new List<string>();
            inst.RefVariables.ForEach(x => refvars.Add(x.ID));
            if (refvars.Count > 0)
                export.RefVars.Value = string.Join(" ", refvars.ToArray());
        }
    }
}

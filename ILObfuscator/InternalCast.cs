using ExchangeFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Internal
{
    public partial class Routine
    {
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
            ex.SetSchemaLocation(@"Schemas\Exchange.xsd");
            root.Description.Value = routine.description;
            
            // Exporting global variables
            foreach (Variable global_var_orig in routine.GlobalVariables)
            {
                VariableType var = root.Global.Last.Variable.Append();
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


        private static void FillData(FunctionType function_export, Function function)
        {
            function_export.CalledFrom.EnumerationValue = function.calledFrom;
            function_export.GlobalID.Value = function.globalID;
            function_export.ID.Value = function.ID;
            List<string> input_vars_id = new List<string>();
            List<string> output_vars_id = new List<string>();
            function.LocalVariables.FindAll(x => x.kind == Variable.Kind.Input).ForEach(x => input_vars_id.Add(x.ID));
            function.LocalVariables.FindAll(x => x.kind == Variable.Kind.Output).ForEach(x => output_vars_id.Add(x.ID));
            if (input_vars_id.Count > 0)
                function_export.RefInputVars.Value = string.Join(" ", input_vars_id.ToArray());
            if (output_vars_id.Count > 0)
                function_export.RefOutputVars.Value = string.Join(" ", output_vars_id.ToArray());

            // Exporting local variables
            if (function.LocalVariables.Count > 0)
                function_export.Local.Append();
            foreach (Variable var_orig in function.LocalVariables)
            {
                VariableType var = function_export.Local.First.Variable.Append();
                FillData(var, var_orig); 
            }

            // Exporting basic blocks
            foreach (BasicBlock bb_orig in function.BasicBlocks)
            {
                BasicBlockType bb = function_export.BasicBlock.Append();
                FillData(bb, bb_orig);
            }
        }


        private static void FillData(VariableType variable_export, Variable variable)
        {
            // Required attributes
            variable_export.ID.Value = variable.ID;
            variable_export.Pointer.Value = variable.pointer;
            variable_export.MemoryRegionSize.Value = variable.memoryRegionSize;
            variable_export.Value = variable.name;

            // Optional Attributes
            variable_export.Fake.Value = variable.fake;
            if (!string.IsNullOrEmpty(variable.fixedValue))
                variable_export.FixedValue.Value = variable.fixedValue;
            if (!string.IsNullOrEmpty(variable.globalID))
                variable_export.GlobalID.Value = variable.globalID;
            if (variable.fixedMax.HasValue)
                variable_export.MaxValue.Value = variable.fixedMax.Value;
            if (variable.fixedMin.HasValue)
                variable_export.MinValue.Value = variable.fixedMin.Value;
            if (variable.memoryUnitSize.HasValue)
                variable_export.MemoryUnitSize.Value = variable.memoryUnitSize.Value;
        }


        private static void FillData(BasicBlockType bb_export, BasicBlock bb)
        {
            bb_export.ID.Value = bb.ID;
            List<string> predecessors = new List<string>();
            List<string> successors = new List<string>();
            bb.getPredecessors.ForEach(x => predecessors.Add(x.ID));
            bb.getSuccessors.ForEach(x => successors.Add(x.ID));
            if (predecessors.Count > 0)
                bb_export.Predecessors.Value = string.Join(" ", predecessors.ToArray());
            if (successors.Count > 0)
                bb_export.Successors.Value = string.Join(" ", successors.ToArray());
            // TODO: continue
            foreach (Instruction inst_original in bb.Instructions)
            {
                InstructionType inst = bb_export.Instruction.Append();
                // FillData(inst, inst_original);
            }
            
        }
    }
}

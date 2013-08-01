using Obfuscator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Text.RegularExpressions;

namespace ObfuscationManager
{
    public static class Validator
    {
        public static bool ValidateXML(XmlDocument doc2validate, out string error_message)
        {
            bool valid = true;
            System.Xml.Schema.XmlSchemaSet schemas = new System.Xml.Schema.XmlSchemaSet();
            schemas.Add(null, @"Schemas\Exchange.xsd");
            XDocument doc = XDocument.Parse(doc2validate.InnerXml);
            string msg = string.Empty;
            doc.Validate(schemas, (o, e) =>
            {
                msg = e.Message;
                valid = false;
            });
            error_message = msg;
            return valid;
        }


        public static Exchange ConvertXMLToExchangeType(XmlDocument doc)
        {
            return Exchange.LoadFromString(doc.InnerXml);
        }


        public static bool ValidateExchangeType(Exchange exch, out string error_message)
        {
            error_message = string.Empty;
            foreach (RoutineType routine in exch.Routine)
            {
                foreach (FunctionType function in routine.Function)
                {
                    //Check ID correctness
                    if (!checkIDcorrectness(function.ID.Value, ref error_message))
                        return false;

                    // Collecting IDs for all variables within a function
                    List<string> variable_IDs = new List<string>();
                    if (function.Inputs.Exists && function.Inputs[0].Original.Exists && function.Inputs[0].Original[0].Variable.Exists)
                        foreach (VariableType var in function.Inputs[0].Original[0].Variable)
                            variable_IDs.Add(var.ID.Value);
                    if (function.Inputs.Exists && function.Inputs[0].Fake.Exists && function.Inputs[0].Fake[0].Variable.Exists)
                        foreach (VariableType var in function.Inputs[0].Fake[0].Variable)
                            variable_IDs.Add(var.ID.Value);
                    if (function.Outputs.Exists && function.Outputs[0].Original.Exists && function.Outputs[0].Original[0].Variable.Exists)
                        foreach (VariableType var in function.Outputs[0].Original[0].Variable)
                            variable_IDs.Add(var.ID.Value);
                    if (function.Outputs.Exists && function.Outputs[0].Fake.Exists && function.Outputs[0].Fake[0].Variable.Exists)
                        foreach (VariableType var in function.Outputs[0].Fake[0].Variable)
                            variable_IDs.Add(var.ID.Value);
                    if (function.Locals.Exists && function.Locals[0].Original.Exists && function.Locals[0].Original[0].Variable.Exists)
                        foreach (VariableType var in function.Locals[0].Original[0].Variable)
                            variable_IDs.Add(var.ID.Value);
                    if (function.Locals.Exists && function.Locals[0].Fake.Exists && function.Locals[0].Fake[0].Variable.Exists)
                        foreach (VariableType var in function.Locals[0].Fake[0].Variable)
                            variable_IDs.Add(var.ID.Value);

                    // Checking 'Variable ID' for correctness
                    foreach (string var in variable_IDs)
                        if (!checkIDcorrectness(var, ref error_message))
                            return false;

                    // Collecting IDs for Basic Blocks and checking the consistence of Predecessors and Successors.
                    List<string> basicblock_IDs = new List<string>();
                    foreach (BasicBlockType bb in function.BasicBlock)
                    {
                        // Checking 'BasicBlock ID' for correctness
                        if (!checkIDcorrectness(bb.ID.Value, ref error_message))
                            return false;

                        basicblock_IDs.Add(bb.ID.Value);
                        // Checking RefVar ID for variables
                        foreach (InstructionType inst in bb.Instruction)
                        {
                            //Checking 'Instruction ID' for correctness
                            if (!checkIDcorrectness(inst.ID.Value, ref error_message))
                                return false;

                            if (inst.RefVars.Exists())
                            {
                                string[] refvars = inst.RefVars.Value.Split(' ');
                                foreach (string refvar in refvars)
                                    if (!variable_IDs.Contains(refvar))
                                    {
                                        error_message = "\nAn instruction contains reference to a non-existing variable.\nAdditional information:\n- Referenced variable (not found): " + refvar + "\n- Instruction: " + inst.ID.Value + ",\n- Basic Block: " + bb.ID.Value + ",\n- Function: " + function.ID.Value + ".";
                                        return false;
                                    }
                            }
                            //Checking correctness of instruction text (TAC instruction)
                            if(!checkTACcorrectness(inst.Value))
                            {
                                error_message = @"TAC instruction is incorrect.\n\tPresent value: '" + inst.Value + @"'";
                                return false;
                            }
                        }
                    }
                    foreach (BasicBlockType bb in function.BasicBlock)
                    {
                        if (bb.Predecessors.Exists())
                        {
                            string[] predecessors = bb.Predecessors.Value.Split(' ');
                            foreach (string pred in predecessors)
                                if (!basicblock_IDs.Contains(pred))
                                {
                                    error_message = "\n'Predecessors' reference is invalid in the basic block.\nAdditional information:\n- Basic block: " + bb.ID.Value + "\n- Predecessor ID (not found): " + pred + "\n- Function:" + function.ID.Value + ".";
                                    return false;
                                }
                        }
                        if (bb.Successors.Exists())
                        {
                            string[] successors = bb.Successors.Value.Split(' ');
                            foreach (string succ in successors)
                                if (!basicblock_IDs.Contains(succ))
                                {
                                    error_message = "\n'Successors' reference is invalid in the basic block.\nAdditional information:\n- Basic block: " + bb.ID.Value + "\n- Successor ID (not found): " + succ + "\n- Function:" + function.ID.Value + ".";
                                    return false;
                                }
                        }
                    }
                }
            }
            return true;
        }

        private static bool checkIDcorrectness(string id, ref string error_message)
        {
            try
            {
                if (Regex.IsMatch(id, @"\bID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b", RegexOptions.None))
                    return true;
                else
                {
                    error_message = "The unique identifier " + id + " is not in a form ID_'GUID'";
                    return false;
                }
            }
            catch (ArgumentException ex)
            {
                error_message = "Failed to validate " + id + ". Additional information: " + ex.Message;
                return false;
            }
        }

        private static bool checkTACcorrectness(string instr)
        {
            string[] operands = instr.Split(' ');
            operands = operands.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            string instr2 = string.Empty;
            foreach (string op in operands)
                instr2 = instr2 + ' ' + op;
            instr2 = instr2.Trim();
            if (string.Equals(instr, instr2))
                return true;
            else
                return false;
        }
    }
}

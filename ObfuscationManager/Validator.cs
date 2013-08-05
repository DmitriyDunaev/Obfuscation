//using ExchangeFormat;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml;
//using System.Xml.Linq;
//using System.Xml.Schema;
//using System.Text.RegularExpressions;

//namespace ObfuscationManager
//{
//    public static class Validator2
//    {
        


//        public static Exchange ConvertXMLToExchangeType(XmlDocument doc)
//        {
//            return Exchange.LoadFromString(doc.InnerXml);
//        }


//        //public static void ValidateExchangeType(Exchange exch)
//        //{
//        //    foreach (RoutineType routine in exch.Routine)
//        //    {
//        //        foreach (FunctionType function in routine.Function)
//        //        {
//        //            //Check ID correctness
//        //            checkIDcorrectness(function.ID.Value);

//        //            //// Collecting IDs for all variables within a function
//        //            //List<string> variable_IDs = new List<string>();
//        //            //if (function.Inputs.Exists && function.Inputs[0].Original.Exists && function.Inputs[0].Original[0].Variable.Exists)
//        //            //    foreach (VariableType var in function.Inputs[0].Original[0].Variable)
//        //            //        variable_IDs.Add(var.ID.Value);
//        //            //if (function.Inputs.Exists && function.Inputs[0].Fake.Exists && function.Inputs[0].Fake[0].Variable.Exists)
//        //            //    foreach (VariableType var in function.Inputs[0].Fake[0].Variable)
//        //            //        variable_IDs.Add(var.ID.Value);
//        //            //if (function.Outputs.Exists && function.Outputs[0].Original.Exists && function.Outputs[0].Original[0].Variable.Exists)
//        //            //    foreach (VariableType var in function.Outputs[0].Original[0].Variable)
//        //            //        variable_IDs.Add(var.ID.Value);
//        //            //if (function.Outputs.Exists && function.Outputs[0].Fake.Exists && function.Outputs[0].Fake[0].Variable.Exists)
//        //            //    foreach (VariableType var in function.Outputs[0].Fake[0].Variable)
//        //            //        variable_IDs.Add(var.ID.Value);
//        //            //if (function.Locals.Exists && function.Locals[0].Original.Exists && function.Locals[0].Original[0].Variable.Exists)
//        //            //    foreach (VariableType var in function.Locals[0].Original[0].Variable)
//        //            //        variable_IDs.Add(var.ID.Value);
//        //            //if (function.Locals.Exists && function.Locals[0].Fake.Exists && function.Locals[0].Fake[0].Variable.Exists)
//        //            //    foreach (VariableType var in function.Locals[0].Fake[0].Variable)
//        //            //        variable_IDs.Add(var.ID.Value);
//        //            //if (routine.RefGlobalVars.Exists())
//        //            //    variable_IDs.AddRange(routine.RefGlobalVars.Value.Split(' '));

//        //            // Checking 'Variable ID' for correctness
//        //            //foreach (string var in variable_IDs)
//        //            //    checkIDcorrectness(var);

//        //            // Collecting IDs for Basic Blocks and checking the consistence of Predecessors and Successors.
//        //            List<string> basicblock_IDs = new List<string>();
//        //            foreach (BasicBlockType bb in function.BasicBlock)
//        //            {
//        //                // If BB has no successors and no predeccessors, it is incorrect
//        //                if (!bb.Successors.Exists() && !bb.Predecessors.Exists())
//        //                    throw new ValidatorException("Basic block " + bb.ID.Value + " has no predecessors and no successors.");

//        //                // Checking 'BasicBlock ID' for correctness
//        //                checkIDcorrectness(bb.ID.Value);

//        //                basicblock_IDs.Add(bb.ID.Value);
//        //                // Checking RefVar ID for variables
//        //                foreach (InstructionType inst in bb.Instruction)
//        //                {
//        //                    //Checking 'Instruction ID' for correctness
//        //                    checkIDcorrectness(inst.ID.Value);

//        //                    //Checking correct number of RefVars by StatementType
//        //                    checkWhitespacesInTAC(inst);

//        //                    // Checking if each RefVar really references an existing variable
//        //                    if (inst.RefVars.Exists())
//        //                    {
//        //                        string[] refvars = inst.RefVars.Value.Split(' ');
//        //                        foreach (string refvar in refvars)
//        //                            if (!variable_IDs.Contains(refvar))
//        //                                throw new ValidatorException("An instruction contains reference to a non-existing variable.\nAdditional information:\n- Referenced variable (not found): " + refvar + "\n- Instruction: " + inst.ID.Value + ",\n- Basic Block: " + bb.ID.Value + ",\n- Function: " + function.ID.Value + ".");
//        //                    }
//        //                }
//        //            }
//        //            foreach (BasicBlockType bb in function.BasicBlock)
//        //            {
//        //                if (bb.Predecessors.Exists())
//        //                {
//        //                    string[] predecessors = bb.Predecessors.Value.Split(' ');
//        //                    foreach (string pred in predecessors)
//        //                        if (!basicblock_IDs.Contains(pred))
//        //                            throw new ValidatorException("'Predecessors' reference is invalid in the basic block.\nAdditional information:\n- Basic block: " + bb.ID.Value + "\n- Predecessor ID (not found): " + pred + "\n- Function:" + function.ID.Value + ".");
//        //                }
//        //                if (bb.Successors.Exists())
//        //                {
//        //                    string[] successors = bb.Successors.Value.Split(' ');
//        //                    foreach (string succ in successors)
//        //                        if (!basicblock_IDs.Contains(succ))
//        //                            throw new ValidatorException("'Successors' reference is invalid in the basic block.\nAdditional information:\n- Basic block: " + bb.ID.Value + "\n- Successor ID (not found): " + succ + "\n- Function:" + function.ID.Value + ".");
//        //                }
//        //            }
//        //        }
//        //    }
//        //}

//        private static void checkIDcorrectness(string id)
//        {
//            // Variable: ^[vtcf]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$
//            // Variable or integer number: ^[vtcf]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$)|(^[-+]?\d+$
//            // Integer (+-): ^[-+]?\d+$
//            // Relop: ^(<|>|>=|<=|==|!=)$
//            // Operator: ^(\+|-|\*|/)$
//            // ID: ^ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$
//                if (Regex.IsMatch(id, @"^ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None))
//                    return;
//                else
//                    throw new ValidatorException("The unique identifier " + id + " is not in a form ID_'GUID'");
//        }

//        private static void checkWhitespacesInTAC(InstructionType inst)
//        {
//            // Checking instruction text (TAC) for being empty
//            if (string.IsNullOrWhiteSpace(inst.Value))
//                throw new ValidatorException("TAC command (text) is empty at instruction " + inst.ID.Value);

//            // TAC instruction must be well-formed 
//            string instr = inst.Value;
//            string[] operands = instr.Split(' ');
//            operands = operands.Where(x => !string.IsNullOrEmpty(x)).ToArray();
//            string instr2 = string.Empty;
//            foreach (string op in operands)
//                instr2 = instr2 + ' ' + op;
//            instr2 = instr2.Trim();
//            if (!string.Equals(instr, instr2))
//                throw new ValidatorException(@"TAC instruction is not well-formed (excess whitespaces?).\n\tPresent value: '" + inst.Value + @"'");
            
//            //// Test 2: Depending on Statement type the number of RefVars is fixed
//            //switch (inst.StatementType.EnumerationValue)
//            //{
//            //    case StatementTypeType.EnumValues.eFullAssignment:
//            //        if (inst.RefVars.Exists() && inst.RefVars.Value.Split(' ').Length >= 1 && inst.RefVars.Value.Split(' ').Length <= 3)
//            //            return;
//            //        break;
//            //    case StatementTypeType.EnumValues.eUnaryAssignment:
//            //    case StatementTypeType.EnumValues.eCopy:
//            //    case StatementTypeType.EnumValues.ePointerAssignment:
//            //        if (inst.RefVars.Exists() && inst.RefVars.Value.Split(' ').Length >= 1 && inst.RefVars.Value.Split(' ').Length <= 2)
//            //            return true;
//            //        break;
//            //    case StatementTypeType.EnumValues.eUnconditionalJump:
//            //        if (!inst.RefVars.Exists())
//            //            return true;
//            //        break;
//            //    case StatementTypeType.EnumValues.eConditionalJump:
//            //        if (!inst.RefVars.Exists() || (inst.RefVars.Exists() && inst.RefVars.Value.Split(' ').Length >= 1 && inst.RefVars.Value.Split(' ').Length <= 2))
//            //            return true;
//            //        break;
//            //    case StatementTypeType.EnumValues.eProcedural:

//            //        // Checking possible instruction types
//            //        string command = inst.Value.Split(' ')[0].ToUpper();
//            //        string[] coms = {"CALL","PARAM", "RETURN", "RETRIEVE", "ENTER", "LEAVE"};
//            //        if (!coms.Contains(command))
//            //        {
//            //            error_message = "Wrong TAC command (text) for procedural instruction " + inst.ID.Value;
//            //            return false;
//            //        }

//            //        if (inst.Value.Split(' ')[0].ToUpper() == "CALL")
//            //        {
//            //            int res = 0;
//            //            if (inst.Value.Split(' ').Length != 3 || !Int32.TryParse(inst.Value.Split(' ')[2], out res))
//            //            {
//            //                error_message = "Wrong TAC command (text) for CALL instruction " + inst.ID.Value;
//            //                return false;
//            //            }
//            //        }

//            //        // Checking presence and number of RefVars
//            //        if (!inst.RefVars.Exists() || (inst.RefVars.Exists() && inst.RefVars.Value.Split(' ').Length == 1 && (
//            //            inst.Value.Split(' ')[0].ToUpper().Equals("PARAM") ||
//            //            inst.Value.Split(' ')[0].ToUpper().Equals("RETURN") ||
//            //            inst.Value.Split(' ')[0].ToUpper().Equals("RETRIEVE"))))
//            //            return true;
//            //        break;
//            //    case StatementTypeType.EnumValues.eIndexedAssignment:
//            //        if (inst.RefVars.Exists() && inst.RefVars.Value.Split(' ').Length >= 2 && inst.RefVars.Value.Split(' ').Length <= 3)
//            //            return true;
//            //        break;
//            //    case StatementTypeType.EnumValues.eUnknown:
//            //        return true;
//            //    default:
//            //        error_message = "Statement type is not found. Internal error. Instruction: " + inst.ID.Value;
//            //        return false;
//            //}
//            //error_message = "Statement type does not match RefVars in instruction: " + inst.ID.Value;
//            //return false;
//        }
//    }


   
//}

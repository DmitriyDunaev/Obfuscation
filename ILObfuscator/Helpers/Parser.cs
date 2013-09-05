using Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Obfuscator
{
    public static class Parser
    {

        /// <summary>
        /// Parses a conditional jump instruction
        /// </summary>
        /// <param name="inst">Instruction to be parsed (conditional jump statement only)</param>
        /// <param name="left_value">Left value of the condition (variable only)</param>
        /// <param name="right_value">Right value of the condition (positive integer only)</param>
        /// <param name="relop">Relational operator in the condition</param>
        /// <param name="true_bb">Basic block the control flow is transfered to if the condition returns TRUE</param>
        /// <param name="false_bb">Basic block the control flow is transfered to if the condition returns FALSE</param>
        public static void ConditionalJump(Instruction inst, out Variable left_value, out int right_value,  out Instruction.RelationalOperationType relop,  out BasicBlock true_bb,  out BasicBlock false_bb)
        {
            Validate(inst, ExchangeFormat.StatementTypeType.EnumValues.eConditionalJump);
            if (inst.RefVariables.Count != 1)
                throw new ParserException("Only conditional jumps with single referenced variable can be parsed.");
            
            // Searching for the left value in Function.LocalVariables list
            left_value = inst.parent.parent.LocalVariables.Find(x => x.ID == inst.RefVariables[0].ID);
            // Searching for the left value in Routine.GlobalVariables list
            if(left_value == null)
                left_value = inst.parent.parent.parent.GlobalVariables.Find(x => x.ID == inst.RefVariables[0].ID);
            // Parsing the right value
            right_value = Convert.ToInt32(Regex.Match(inst.TACtext, @"(^\d+)|(\s\d+)").Value.Trim());
            string relop_sign = Regex.Match(inst.TACtext, @"( == )|( != )|( > )|( >= )|( < )|( <= )").Value.Trim();
            switch (relop_sign)
            {
                case "==":
                    relop = Instruction.RelationalOperationType.Equals;
                    break;
                case "!=":
                    relop = Instruction.RelationalOperationType.NotEquals;
                    break;
                case ">":
                    relop = Instruction.RelationalOperationType.Greater;
                    break;
                case ">=":
                    relop = Instruction.RelationalOperationType.GreaterOrEquals;
                    break;
                case "<":
                    relop = Instruction.RelationalOperationType.Smaller;
                    break;
                case "<=":
                    relop = Instruction.RelationalOperationType.SmallerOrEquals;
                    break;
                default:
                    throw new ParserException("Cannot parse relational operator.");
            }

            // True basic block
            true_bb = inst.parent.getSuccessors.First();
            // False basic block
            false_bb = inst.parent.getSuccessors.Last();

        }


        /// <summary>
        /// Parses an unconditional jump instruction (goto)
        /// </summary>
        /// <param name="inst">Instruction to be parsed</param>
        /// <param name="target">Basic block to which the control flow is transfered</param>
        public static void UnconditionalJump(Instruction inst, out BasicBlock target)
        {
            target = inst.parent.getSuccessors.First();
        }


        /// <summary>
        /// Parses a full assignment instruction
        /// </summary>
        /// <param name="inst">Instruction to be parsed</param>
        /// <param name="left_value">Left value of the assignment (variable only)</param>
        /// <param name="right_value1">Right value before arithmetic operator</param>
        /// <param name="right_value2">Right value after arithmetic operator (for variable), null for number</param>
        /// <param name="right_value_int">Right value after arithmetic operator (for number), null for variable</param>
        /// <param name="operation">Arithmetic operator</param>
        public static void FullAssignment(Instruction inst, out Variable left_value, out Variable right_value1, out Variable right_value2, out int? right_value_int, out Instruction.ArithmeticOperationType operation)
        {
            Validate(inst, ExchangeFormat.StatementTypeType.EnumValues.eFullAssignment);
            System.Collections.Specialized.StringCollection refvarIDs = new System.Collections.Specialized.StringCollection();
            Match matchResult = Regex.Match(inst.TACtext, "ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}", RegexOptions.None);
            while (matchResult.Success)
            {
                refvarIDs.Add(matchResult.Value);
                matchResult = matchResult.NextMatch();
            }

            left_value = inst.RefVariables.Find(x => x.ID == refvarIDs[0]);
            right_value1 = inst.RefVariables.Find(x => x.ID == refvarIDs[1]);
            if (refvarIDs.Count == 3)
            {
                right_value2 = inst.RefVariables.Find(x => x.ID == refvarIDs[2]);
                right_value_int = null;
            }
            else
            {
                right_value_int = Convert.ToInt32(inst.TACtext.Split(' ')[4]);
                right_value2 = null;
            }
            switch (inst.TACtext.Split(' ')[3])
            {
                case "+":
                    operation = Instruction.ArithmeticOperationType.Addition;
                    break;
                case "-":
                    operation = Instruction.ArithmeticOperationType.Subtraction;
                    break;
                case "*":
                    operation = Instruction.ArithmeticOperationType.Multiplication;
                    break;
                case @"/":
                    operation = Instruction.ArithmeticOperationType.Division;
                    break;
                default:
                    throw new ParserException("Cannot parse arithmetic operation type.");
            }
        }


        /// <summary>
        /// Parses an unary assignment instruction
        /// </summary>
        /// <param name="inst">Instruction to be parsed</param>
        /// <param name="left_value">Left value of the assignment (variable only)</param>
        /// <param name="right_value">Right value of the assignment (variable only)</param>
        /// <param name="operation">Unary operator</param>
        public static void UnaryAssignment(Instruction inst, out Variable left_value, out Variable right_value, out Instruction.UnaryOperationType operation)
        {
            Validate(inst, ExchangeFormat.StatementTypeType.EnumValues.eUnaryAssignment);
            System.Collections.Specialized.StringCollection refvarIDs = new System.Collections.Specialized.StringCollection();
            Match matchResult = Regex.Match(inst.TACtext, "ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}", RegexOptions.None);
            while (matchResult.Success)
            {
                refvarIDs.Add(matchResult.Value);
                matchResult = matchResult.NextMatch();
            }

            left_value = inst.RefVariables.Find(x => x.ID == refvarIDs[0]);
            right_value = inst.RefVariables.Find(x => x.ID == refvarIDs[1]);
            
            switch (inst.TACtext.Split(' ')[2])
            {
                case "-":
                    operation = Instruction.UnaryOperationType.ArithmeticNegation;
                    break;
                case "!":
                    operation = Instruction.UnaryOperationType.LogicalNegation;
                    break;
                default:
                    throw new ObfuscatorException("Unsupported unary operation type.");
            }
        }


        /// <summary>
        /// Parses a copy instruction
        /// </summary>
        /// <param name="inst">Instruction to be parsed</param>
        /// <param name="left_value">Left value of the copy (copy to)</param>
        /// <param name="right_value">Right value of the copy (copy from), variable</param>
        /// <param name="right_value_int">Right value of the copy (copy from), number</param>
        public static void Copy(Instruction inst, Variable left_value, Variable right_value, int? right_value_int)
        {
            Validate(inst, ExchangeFormat.StatementTypeType.EnumValues.eCopy);
            System.Collections.Specialized.StringCollection refvarIDs = new System.Collections.Specialized.StringCollection();
            Match matchResult = Regex.Match(inst.TACtext, "ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}", RegexOptions.None);
            while (matchResult.Success)
            {
                refvarIDs.Add(matchResult.Value);
                matchResult = matchResult.NextMatch();
            }
            left_value = inst.RefVariables.Find(x => x.ID == refvarIDs[0]);
            if (refvarIDs.Count == 2)
            {
                left_value = inst.RefVariables.Find(x => x.ID == refvarIDs[0]);
                right_value_int = null;
            }
            else
            {
                left_value = null;
                right_value_int = Convert.ToInt32(inst.TACtext.Split(' ')[2]);
            }
        }


        /// <summary>
        /// Parses a procedural instruction
        /// </summary>
        /// <param name="inst">Instruction to be parsed</param>
        /// <param name="var">Variable as a parameter</param>
        /// <param name="num">Number as a parameter</param>
        /// <param name="operation">Procedural operation type</param>
        /// <param name="called_func">Called function (only for CALL type)</param>
        public static void Procedural(Instruction inst, out Variable var, out int? num, out Instruction.ProceduralType operation, out Function called_func)
        {
            Validate(inst, ExchangeFormat.StatementTypeType.EnumValues.eProcedural);
            num = null;
            var = null;
            called_func = null;
            switch (inst.TACtext.Split(' ')[0])
            {
                case "param":
                    operation = Instruction.ProceduralType.Param;
                    if (Regex.Match(inst.TACtext, "ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}", RegexOptions.None).Success)
                        var = inst.RefVariables.Find(x => x.ID == Regex.Match(inst.TACtext, "ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}", RegexOptions.None).Value);
                    else
                        num = Convert.ToInt32(inst.TACtext.Split(' ')[1]);
                    break;
                case "call":
                    operation = Instruction.ProceduralType.Call;
                    called_func = inst.parent.parent.parent.Functions.Find(x => x.ID == inst.TACtext.Split(' ')[1]);
                    num = Convert.ToInt32(inst.TACtext.Split(' ')[2]);
                    break;
                case "return":
                    operation = Instruction.ProceduralType.Return;
                    if (inst.TACtext.Split(' ').Length == 2)
                    {
                        if (Regex.Match(inst.TACtext, "ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}", RegexOptions.None).Success)
                            var = inst.RefVariables.Find(x => x.ID == Regex.Match(inst.TACtext, "ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}", RegexOptions.None).Value);
                        else
                            num = Convert.ToInt32(inst.TACtext.Split(' ')[1]);
                    }
                    break;
                case "retrieve":
                    operation = Instruction.ProceduralType.Retrieve;
                    var = inst.RefVariables.Find(x => x.ID == Regex.Match(inst.TACtext, "ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}", RegexOptions.None).Value);
                    break;
                default:
                    throw new ParserException("Cannot parse TAC text in procedural instruction " + inst.ID);
            }

        }


        /// <summary>
        /// Parses a pointer assignment instruction
        /// </summary>
        /// <param name="inst">Instruction to be parsed</param>
        /// <param name="left_value">Left value of the assignment (assign to)</param>
        /// <param name="right_value">Right value of the assignment, variable</param>
        /// <param name="right_value_int">Right value of the assignment, number</param>
        /// <param name="operation"></param>
        public static void PointerAssignment(Instruction inst, out Variable left_value, out Variable right_value, out int? right_value_num, out Instruction.PoinerType operation)
        {
            throw new NotImplementedException("Parser.PointerAssignment is not implemented yet!");
        }

        
        
        private static void Validate(Instruction inst, ExchangeFormat.StatementTypeType.EnumValues statement)
        {
            if (inst.statementType != statement)
                throw new ParserException("Instruction's statement type does not comply with parser. Instruction: " + inst.ID);
            inst.Validate();
            if (inst.parent == null || inst.parent.parent == null || inst.parent.parent.parent == null)
                throw new ParserException("Instruction's 'parent' or 'parent.parent' or 'parent.parent.parent' property is invalid. Instruction: " + inst.ID);
        }


        
    }
}

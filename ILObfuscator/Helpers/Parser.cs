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
                    throw new ObfuscatorException("Unrecognized relational operator.");
            }

            // True basic block
            true_bb = inst.parent.getSuccessors.First();
            // False basic block
            false_bb = inst.parent.getSuccessors.Last();

        }


        public static void UnconditionalJump(Instruction inst, out BasicBlock target)
        {
            target = inst.parent.getSuccessors.First();
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

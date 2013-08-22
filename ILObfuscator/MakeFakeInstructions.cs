using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Obfuscator;

namespace Internal
{
    public partial class Instruction
    {

        /// <summary>
        /// Makes FullAssignment instruction type from NoOperation
        /// </summary>
        /// <param name="left_value">Left value (variable only)</param>
        /// <param name="right_value1">Right value before operator</param>
        /// <param name="right_value2">Right value after operator (for variable), null for number</param>
        /// <param name="right_value_int">Right value after operator (for number), null for variable</param>
        /// <param name="operation">Desired arithmetic operation</param>
        public void MakeFullAssignment(Variable left_value, Variable right_value1, Variable right_value2, int? right_value_int, ArithmeticOperationType operation)
        {
            if (statementType != ExchangeFormat.StatementTypeType.EnumValues.eNoOperation)
                throw new ObfuscatorException("Only NoOperation instruction can be modified to other type!");

            if (right_value2 == null && right_value_int == null)
                throw new ObfuscatorException("Wrong parameter passing: one of the second right value and the constant must be null.");
            if (right_value2 != null && right_value_int != null)
                throw new ObfuscatorException("Wrong parameter passing: both second right value and constant is null.");
            if (left_value == null)
                throw new ObfuscatorException("Wrong parameter passing: left value missing.");
            if (right_value1 == null)
                throw new ObfuscatorException("Wrong parameter passing: first right value missing.");

            string left1 = left_value.name;
            string right1 = right_value1.name;
            string right2 = right_value_int == null ? right_value2.name : right_value_int.ToString();
            string op;
            switch (operation)
            {
                case ArithmeticOperationType.Addition:
                    op = "+";
                    break;
                case ArithmeticOperationType.Subtraction:
                    op = "-";
                    break;
                case ArithmeticOperationType.Multiplication:
                    op = "*";
                    break;
                case ArithmeticOperationType.Division:
                    op = @"/";
                    break;
                default:
                    throw new ObfuscatorException("Unsupported arithmetic operation type.");
            }
            RefVariables.Clear();
            RefVariables.Add(left_value);
            RefVariables.Add(right_value1);
            if (right_value_int == null)
                RefVariables.Add(right_value2);
            statementType = ExchangeFormat.StatementTypeType.EnumValues.eFullAssignment;
            TACtext = string.Join(" ", left1, ":=", right1, op, right2);
        }


        /// <summary>
        /// Makes UnaryAssignment instruction type from NoOperation
        /// </summary>
        /// <param name="left_value">Left value (variable only)</param>
        /// <param name="right_value">Right value (variable only, can be the same as a left value)</param>
        /// <param name="operation">Unary operation</param>
        public void MakeUnaryAssignment(Variable left_value, Variable right_value, UnaryOperationType operation)
        {
            if (statementType != ExchangeFormat.StatementTypeType.EnumValues.eNoOperation)
                throw new ObfuscatorException("Only NoOperation instruction can be modified to other type!");

            if (left_value == null)
                throw new ObfuscatorException("Wrong parameter passing: missing left value.");
            if (right_value == null)
                throw new ObfuscatorException("Wrong parameter passing: missing right value.");

            string op = string.Empty;
            switch (operation)
            {
                case UnaryOperationType.ArithmeticNegation:
                    op = "-";
                    break;
                case UnaryOperationType.LogicalNegation:
                    op = "!";
                    break;
                default:
                    throw new ObfuscatorException("Unsupported unary operation type.");
            }
            RefVariables.Clear();
            RefVariables.Add(left_value);
            if (!left_value.Equals(right_value))
                RefVariables.Add(right_value);
            statementType = ExchangeFormat.StatementTypeType.EnumValues.eUnaryAssignment;
            TACtext = string.Join(" ", left_value.name, ":=", op, right_value.name);
        }



        /// <summary>
        /// Makes Copy instruction type from NoOperation
        /// </summary>
        /// <param name="left_value">Left value (variable only)</param>
        /// <param name="right_value">Right value (variable), null for number</param>
        /// <param name="right_value_int">Right value (number), null for variable</param>
        public void MakeCopy(Variable left_value, Variable right_value, int? right_value_int)
        {
            if (statementType != ExchangeFormat.StatementTypeType.EnumValues.eNoOperation)
                throw new ObfuscatorException("Only NoOperation instruction can be modified to other type!");

            if (left_value == null)
                throw new ObfuscatorException("Wrong parameter passing: missing left value.");
            if (right_value == null && right_value_int == null)
                throw new ObfuscatorException("Wrong parameter passing: both right value and constant is null.");
            if (right_value != null && right_value_int != null)
                throw new ObfuscatorException("Wrong parameter passing: one of the right value and the constant must be null.");

            RefVariables.Clear();
            RefVariables.Add(left_value);
            if (right_value_int == null)
                RefVariables.Add(right_value);
            statementType = ExchangeFormat.StatementTypeType.EnumValues.eCopy;
            TACtext = right_value_int == null ? string.Join(" ", left_value.name, ":=", right_value.name) : string.Join(" ", left_value.name, ":=", right_value_int);
        }


        /// <summary>
        /// Makes ConditionalJump instruction type from NoOperation (+ links Successors and Predecessors)
        /// </summary>
        /// <param name="left_value">Left value in relation (only variable)</param>
        /// <param name="right_value">Left value in relation (only numerical value)</param>
        /// <param name="relop">Relational operation</param>
        /// <param name="target">Target basic block the control flow is transfered to, if the relation holds true</param>
        public void MakeConditionalJump(Variable left_value, int right_value, RelationalOperationType relop, BasicBlock target)
        {
            if (statementType != ExchangeFormat.StatementTypeType.EnumValues.eNoOperation && statementType != ExchangeFormat.StatementTypeType.EnumValues.eUnconditionalJump)
                throw new ObfuscatorException("Only NoOperation or UnconditionalJump instructions can be modified to ConditionalJump type!");

            if (target.parent != parent.parent)
                throw new ObfuscatorException("Target basic block and original are in different functions.");

            if (parent.getSuccessors.Count != 1)
                throw new ObfuscatorException("The basic block should have exactly one successor.");

            if (left_value == null)
                throw new ObfuscatorException("Wrong parameter passing.");

            if (!parent.Instructions.Last().Equals(this))
                throw new ObfuscatorException("Only the last instruction of a basic block can be modified to ConditionalJump.");

            RefVariables.Add(left_value);
            statementType = ExchangeFormat.StatementTypeType.EnumValues.eConditionalJump;
            string strRelop = string.Empty;
            switch (relop)
            {
                case RelationalOperationType.Equals:
                    strRelop = "==";
                    break;
                case RelationalOperationType.NotEquals:
                    strRelop = "!=";
                    break;
                case RelationalOperationType.Greater:
                    strRelop = ">";
                    break;
                case RelationalOperationType.GreaterOrEquals:
                    strRelop = ">=";
                    break;
                case RelationalOperationType.Smaller:
                    strRelop = "<";
                    break;
                case RelationalOperationType.SmallerOrEquals:
                    strRelop = "<=";
                    break;
                default:
                    throw new ObfuscatorException("Unsupported relational operator type.");
            }
            TACtext = string.Join(" ", "if", left_value.name, strRelop, right_value, "goto", target.ID);
            parent.LinkToSuccessor(target, false, true);
        }


        /// <summary>
        /// Makes UnconditionalJump instruction type from NoOperation (+ links Successors and Predecessors)
        /// </summary>
        /// <param name="target">Target basic block the control flow is transfered to after 'goto'</param>
        public void MakeUnconditionalJump(BasicBlock target)
        {
            if (statementType != ExchangeFormat.StatementTypeType.EnumValues.eNoOperation)
                throw new ObfuscatorException("Only NoOperation instruction can be modified to other type!");

            if (target == null || this.parent == null || this.parent.parent != target.parent)
                throw new ObfuscatorException("Wrong parameter passing.");

            if (!parent.Instructions.Last().Equals(this))
                throw new ObfuscatorException("Only the last NoOperation instruction of a basic block can be modified to UnconditionalJump.");

            statementType = ExchangeFormat.StatementTypeType.EnumValues.eUnconditionalJump;
            TACtext = string.Join(" ", "goto", target.ID);
            parent.LinkToSuccessor(target, true);
        }
    }
}

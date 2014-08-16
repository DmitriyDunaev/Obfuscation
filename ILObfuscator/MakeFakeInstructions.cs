using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Objects;
using Services;

namespace Obfuscator
{
    public static class MakeInstruction
    {

        /// <summary>
        /// Makes FullAssignment instruction type from NoOperation
        /// </summary>
        /// <param name="left_value">Left value (variable only)</param>
        /// <param name="right_value1">Right value before operator</param>
        /// <param name="right_value2">Right value after operator (for variable), null for number</param>
        /// <param name="right_value_int">Right value after operator (for number), null for variable</param>
        /// <param name="operation">Desired arithmetic operation</param>
        public static void FullAssignment(Instruction ins, Variable left_value, Variable right_value1, Variable right_value2, int? right_value_int, Instruction.ArithmeticOperationType operation)
        {
            if (ins.statementType != Objects.Common.StatementType.NoOperation)
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
                case Instruction.ArithmeticOperationType.Addition:
                    op = "+";
                    break;
                case Instruction.ArithmeticOperationType.Subtraction:
                    op = "-";
                    break;
                case Instruction.ArithmeticOperationType.Multiplication:
                    op = "*";
                    break;
                case Instruction.ArithmeticOperationType.Division:
                    op = @"/";
                    break;
                default:
                    throw new ObfuscatorException("Unsupported arithmetic operation type.");
            }
            ins.RefVariables.Clear();
            ins.RefVariables.Add(left_value);
            ins.RefVariables.Add(right_value1);
            if (right_value_int == null)
                ins.RefVariables.Add(right_value2);
            ins.statementType = Objects.Common.StatementType.FullAssignment;
            ins.TACtext = string.Join(" ", left1, ":=", right1, op, right2);
        }


        /// <summary>
        /// Makes UnaryAssignment instruction type from NoOperation
        /// </summary>
        /// <param name="left_value">Left value (variable only)</param>
        /// <param name="right_value">Right value (variable only, can be the same as a left value)</param>
        /// <param name="operation">Unary operation</param>
        public static void UnaryAssignment(Instruction ins, Variable left_value, Variable right_value, Instruction.UnaryOperationType operation)
        {
            if (ins.statementType != Objects.Common.StatementType.NoOperation)
                throw new ObfuscatorException("Only NoOperation instruction can be modified to other type!");

            if (left_value == null)
                throw new ObfuscatorException("Wrong parameter passing: missing left value.");
            if (right_value == null)
                throw new ObfuscatorException("Wrong parameter passing: missing right value.");

            string op = string.Empty;
            switch (operation)
            {
                case Instruction.UnaryOperationType.ArithmeticNegation:
                    op = "-";
                    break;
                case Instruction.UnaryOperationType.LogicalNegation:
                    op = "!";
                    break;
                default:
                    throw new ObfuscatorException("Unsupported unary operation type.");
            }
            ins.RefVariables.Clear();
            ins.RefVariables.Add(left_value);
            if (!left_value.Equals(right_value))
                ins.RefVariables.Add(right_value);
            ins.statementType = Objects.Common.StatementType.UnaryAssignment;
            ins.TACtext = string.Join(" ", left_value.name, ":=", op, right_value.name);
        }



        /// <summary>
        /// Makes Copy instruction type from NoOperation
        /// </summary>
        /// <param name="left_value">Left value (variable only)</param>
        /// <param name="right_value">Right value (variable), null for number</param>
        /// <param name="right_value_int">Right value (number), null for variable</param>
        public static void Copy(Instruction ins, Variable left_value, Variable right_value, int? right_value_int)
        {
            if (ins.statementType != Objects.Common.StatementType.NoOperation)
                throw new ObfuscatorException("Only NoOperation instruction can be modified to other type!");

            if (left_value == null)
                throw new ObfuscatorException("Wrong parameter passing: missing left value.");
            if (right_value == null && right_value_int == null)
                throw new ObfuscatorException("Wrong parameter passing: both right value and constant is null.");
            if (right_value != null && right_value_int != null)
                throw new ObfuscatorException("Wrong parameter passing: one of the right value and the constant must be null.");

            ins.RefVariables.Clear();
            ins.RefVariables.Add(left_value);
            if (right_value_int == null)
                ins.RefVariables.Add(right_value);
            ins.statementType = Objects.Common.StatementType.Copy;
            ins.TACtext = right_value_int == null ? string.Join(" ", left_value.name, ":=", right_value.name) : string.Join(" ", left_value.name, ":=", right_value_int);
        }


        /// <summary>
        /// Makes ConditionalJump instruction type from NoOperation (+ links Successors and Predecessors)
        /// </summary>
        /// <param name="left_value">Left value in relation (only variable)</param>
        /// <param name="right_value">Left value in relation (only numerical value)</param>
        /// <param name="relop">Relational operation</param>
        /// <param name="target">Target basic block the control flow is transfered to, if the relation holds true</param>
        public static void ConditionalJump(Instruction ins, Variable left_value, int right_value, Instruction.RelationalOperationType relop, BasicBlock target)
        {
            if (ins.statementType != Objects.Common.StatementType.NoOperation && ins.statementType != Objects.Common.StatementType.UnconditionalJump)
                throw new ObfuscatorException("Only NoOperation or UnconditionalJump instructions can be modified to ConditionalJump type!");

            if (target.parent != ins.parent.parent)
                throw new ObfuscatorException("Target basic block and original are in different functions.");

            if (ins.parent.getSuccessors.Count != 1)
                throw new ObfuscatorException("The basic block should have exactly one successor.");

            if (left_value == null)
                throw new ObfuscatorException("Wrong parameter passing.");

            if (!ins.parent.Instructions.Last().Equals(ins))
                throw new ObfuscatorException("Only the last instruction of a basic block can be modified to ConditionalJump.");

            ins.RefVariables.Add(left_value);
            ins.statementType = Objects.Common.StatementType.ConditionalJump;
            string strRelop = string.Empty;
            switch (relop)
            {
                case Instruction.RelationalOperationType.Equals:
                    strRelop = "==";
                    break;
                case Instruction.RelationalOperationType.NotEquals:
                    strRelop = "!=";
                    break;
                case Instruction.RelationalOperationType.Greater:
                    strRelop = ">";
                    break;
                case Instruction.RelationalOperationType.GreaterOrEquals:
                    strRelop = ">=";
                    break;
                case Instruction.RelationalOperationType.Smaller:
                    strRelop = "<";
                    break;
                case Instruction.RelationalOperationType.SmallerOrEquals:
                    strRelop = "<=";
                    break;
                default:
                    throw new ObfuscatorException("Unsupported relational operator type.");
            }
            ins.TACtext = string.Join(" ", "if", left_value.name, strRelop, right_value, "goto", target.ID);
            ins.parent.LinkToSuccessor(target, false, true);
        }


        /// <summary>
        /// Makes random conditional jump instruction + links basic blocks and sets RefVars
        /// </summary>
        /// <param name="nop">NoOperation instruction (will be made into ConditionalJump)</param>
        /// <param name="condition">Type of condition</param>
        /// <param name="target">Target basic block the control flow is transfered to, if the relation holds true.</param>
        public static void RandomConditionalJump(Instruction nop, Instruction.ConditionType condition, BasicBlock target)
        {
            if (nop.statementType != Objects.Common.StatementType.NoOperation && nop.statementType != Objects.Common.StatementType.UnconditionalJump)
                throw new ObfuscatorException("Only NoOperation and UnconditionalJump instructions can be modified to ConditionalJump!");
            if (nop.parent == null || nop.parent.parent == null)
                throw new ObfuscatorException("Instruction -> basic block -> function parent link is broken.");
            if (nop.parent.parent != target.parent)
                throw new ObfuscatorException("The instruction and the basic block should be contained in the same function.");

            Variable var = FakeParameters.GetRandom(nop.parent.parent);
            if (var.fixedMax.HasValue && var.fixedMax.Value > Common.GlobalMaxValue)
                throw new ObfuscatorException("The fixedMax value is greated then the globally accepted maximum.");
            if (var.fixedMin.HasValue && var.fixedMin.Value < Common.GlobalMinValue)
                throw new ObfuscatorException("The fixedMin value is smaller then the globally accepted minimum.");
            int right_value = 0;
            Instruction.RelationalOperationType relop = Instruction.RelationalOperationType.Equals;
            bool use_min_limit = false;
            // Here we chose to use a FixedMin or FixedMax for logical relation
            if (var.fixedMin.HasValue && var.fixedMax.HasValue)
                use_min_limit = (bool)Randomizer.OneFromMany(true, false);
            else if (var.fixedMin.HasValue)
                use_min_limit = true;

            if (use_min_limit)  // FixedMin will be used
            {
                right_value = Randomizer.OneFromSectionWithDescendingProbability(var.fixedMin.Value, Common.GlobalMinValue + Common.LoopConditionalJumpMaxRange);
                switch (condition)
                {
                    case Instruction.ConditionType.AlwaysTrue:
                        relop = (Instruction.RelationalOperationType)Randomizer.OneFromMany(Instruction.RelationalOperationType.Greater,
                                                                                    Instruction.RelationalOperationType.GreaterOrEquals);
                        break;
                    case Instruction.ConditionType.AlwaysFalse:
                        relop = (Instruction.RelationalOperationType)Randomizer.OneFromMany(Instruction.RelationalOperationType.Smaller,
                                                                                    Instruction.RelationalOperationType.SmallerOrEquals);
                        break;
                    case Instruction.ConditionType.Random:
                        relop = (Instruction.RelationalOperationType)Randomizer.OneFromMany(Instruction.RelationalOperationType.Smaller,
                                                                                    Instruction.RelationalOperationType.SmallerOrEquals,
                                                                                    Instruction.RelationalOperationType.Greater,
                                                                                    Instruction.RelationalOperationType.GreaterOrEquals);
                        break;
                    default:
                        throw new ObfuscatorException("Unrecognized condition type.");
                }
            }

            if (!use_min_limit)     // FixedMax will be used
            {
                right_value = Randomizer.OneFromSectionWithDescendingProbability(var.fixedMax.Value, Common.GlobalMaxValue - Common.LoopConditionalJumpMaxRange);
                switch (condition)
                {
                    case Instruction.ConditionType.AlwaysTrue:
                        relop = (Instruction.RelationalOperationType)Randomizer.OneFromMany(Instruction.RelationalOperationType.Smaller,
                                                                                    Instruction.RelationalOperationType.SmallerOrEquals);
                        break;
                    case Instruction.ConditionType.AlwaysFalse:
                        relop = (Instruction.RelationalOperationType)Randomizer.OneFromMany(Instruction.RelationalOperationType.Greater,
                                                                                    Instruction.RelationalOperationType.GreaterOrEquals);
                        break;
                    case Instruction.ConditionType.Random:
                        relop = (Instruction.RelationalOperationType)Randomizer.OneFromMany(Instruction.RelationalOperationType.Smaller,
                                                                                    Instruction.RelationalOperationType.SmallerOrEquals,
                                                                                    Instruction.RelationalOperationType.Greater,
                                                                                    Instruction.RelationalOperationType.GreaterOrEquals);
                        break;
                    default:
                        throw new ObfuscatorException("Unrecognized condition type.");
                }
            }
            MakeInstruction.ConditionalJump(nop, var, right_value, relop, target);
        }





        /// <summary>
        /// Makes UnconditionalJump instruction type from NoOperation (+ links Successors and Predecessors)
        /// </summary>
        /// <param name="target">Target basic block the control flow is transfered to after 'goto'</param>
        public static void UnconditionalJump(Instruction ins, BasicBlock target)
        {
            if (ins.statementType != Objects.Common.StatementType.NoOperation)
                throw new ObfuscatorException("Only NoOperation instruction can be modified to other type!");

            if (target == null || ins.parent == null || ins.parent.parent != target.parent)
                throw new ObfuscatorException("Wrong parameter passing.");

            if (!ins.parent.Instructions.Last().Equals(ins))
                throw new ObfuscatorException("Only the last NoOperation instruction of a basic block can be modified to UnconditionalJump.");

            ins.statementType = Objects.Common.StatementType.UnconditionalJump;
            ins.TACtext = string.Join(" ", "goto", target.ID);
            ins.parent.LinkToSuccessor(target, true);
        }


        /// <summary>
        /// Makes 'param' procedural instruction type
        /// </summary>
        /// <param name="value">Parameter value</param>
        public static void ProceduralParam(Instruction ins, Variable var, int? value)
        {
            if (ins.statementType != Objects.Common.StatementType.NoOperation)
                throw new ObfuscatorException("Only NoOperation instruction can be modified to other type!");

            if ((var == null && !value.HasValue) || (var != null && value.HasValue))
                throw new ObfuscatorException("Wrong parameter passing.");

            ins.statementType = Objects.Common.StatementType.Procedural;
            if (value.HasValue)
            {
                ins.TACtext = string.Join(" ", "param", value.Value.ToString());
            }
            else
            {
                ins.RefVariables.Add(var);
                ins.TACtext = string.Join(" ", "param", var.name);
            }
        }
    }
}

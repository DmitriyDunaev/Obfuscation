using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Internal;
using Obfuscator;


namespace Platform_x86
{
    public static class Assembler
    {

        public static string GetAssemblyFromTAC(Function func)
        {
            StringBuilder sb = new StringBuilder();

            Dictionary<Variable, int> offsets = new Dictionary<Variable, int>();
            int framestack = BuildStack(func, offsets);
            sb.AppendLine(Prologue(func, framestack));

            Obfuscator.Traversal.ReorderBasicBlocks(func);
            foreach (BasicBlock bb in func.BasicBlocks)
            {
                foreach (Instruction inst in bb.Instructions)
                {
                    switch (inst.statementType)
                    {
                        case ExchangeFormat.StatementTypeType.EnumValues.eFullAssignment:
                            break;
                        case ExchangeFormat.StatementTypeType.EnumValues.eUnaryAssignment:
                            break;
                        case ExchangeFormat.StatementTypeType.EnumValues.eCopy:
                            break;
                        case ExchangeFormat.StatementTypeType.EnumValues.eUnconditionalJump:
                            sb.AppendLine(UnconditionalJump(inst, inst.polyRequired));
                            break;
                        case ExchangeFormat.StatementTypeType.EnumValues.eConditionalJump:
                            sb.AppendLine(ConditionalJump(inst, inst.polyRequired));
                            break;
                        case ExchangeFormat.StatementTypeType.EnumValues.eProcedural:
                            break;
                        case ExchangeFormat.StatementTypeType.EnumValues.eIndexedAssignment:
                            break;
                        case ExchangeFormat.StatementTypeType.EnumValues.ePointerAssignment:
                            break;
                        case ExchangeFormat.StatementTypeType.EnumValues.eNoOperation:
                            sb.AppendLine(NoOperation(inst, inst.polyRequired));
                            break;
                        case ExchangeFormat.StatementTypeType.EnumValues.Invalid:
                            break;
                        default:
                            throw new AssemblerException("Statement type is not supported by Assembler.");
                    }

                }
            }

            sb.AppendLine(Epilogue(func, framestack));

            return sb.ToString();
        }


        private static string NoOperation(Instruction inst, bool polyReq = false)
        {
            return "NOP";
        }

        private static string ConditionalJump(Instruction inst, bool polyReq = false)
        {
            StringBuilder sb = new StringBuilder();

            /* We parse the conditional jump. */
            Variable left;
            int right;
            Instruction.RelationalOperationType relop;
            BasicBlock truebb, falsebb;
            inst.ConditionalJumpInstruction(out left, out right, out relop, out truebb, out falsebb);
            
            /* Now we build the assembly instructions. */
            sb.AppendLine("MOV eax [ebp + " + /*offset[left]*/5 + "]");
            sb.AppendLine("CMP eax " + right);
            switch (relop)
            {
                case Instruction.RelationalOperationType.Equals:
                    sb.Append("JE ");
                    break;
                case Instruction.RelationalOperationType.Greater:
                    sb.Append("JG ");
                    break;
                case Instruction.RelationalOperationType.GreaterOrEquals:
                    sb.Append("JGE ");
                    break;
                case Instruction.RelationalOperationType.NotEquals:
                    sb.Append("JNE ");
                    break;
                case Instruction.RelationalOperationType.Smaller:
                    sb.Append("JL ");
                    break;
                case Instruction.RelationalOperationType.SmallerOrEquals:
                    sb.Append("JLE ");
                    break;
            }
            sb.AppendLine(truebb.ID);

            return sb.ToString();
        }

        private static string UnconditionalJump(Instruction inst, bool polyReq = false)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("jmp ");
            sb.Append(inst.TACtext.Split(' ')[1]);

            return sb.ToString();
        }


        private static string Prologue(Function func, int framestack)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(".ent " + func.globalID);
            sb.AppendLine(func.globalID + ":");
            sb.AppendLine("subu $sp, " + framestack);
            return sb.ToString();
        }

        private static int BuildStack(Function func, Dictionary<Variable, int> offsets)
        {
            int fs = 0;
            foreach (Variable var in func.LocalVariables)
            {
                offsets.Add(var, fs);
                fs += var.memoryRegionSize;
            }
            return fs;
        }

        private static string Epilogue(Function func, int framestack)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("addu $sp, " + framestack);
            return sb.ToString();
        }


    }







    [Serializable]
    public class AssemblerException : Exception
    {
        public AssemblerException() { }
        public AssemblerException(string message) : base(message) { }
        public AssemblerException(string message, Exception inner) : base(message, inner) { }
        protected AssemblerException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }






}

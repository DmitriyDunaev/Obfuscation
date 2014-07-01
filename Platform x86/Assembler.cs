using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Internal;
using Obfuscator;
using System.Text.RegularExpressions;


namespace Platform_x86
{
    public static class Assembler
    {
        /// <summary>
        /// Dictionary to store the offsets of the variables on the stack.
        /// </summary>
        private static Dictionary<Variable, int> Offsets = new Dictionary<Variable, int>();

        private static Dictionary<string, string> ReadableBBLabels = new Dictionary<string, string>();
        private static int counter = 0;

        private static int framestack;

        public static string GetAssemblyFromTAC(Routine routine)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(".386");
            sb.AppendLine(".model flat, stdcall");
            sb.AppendLine();
            sb.AppendLine("include \\masm32\\include\\msvcrt.inc");
            sb.AppendLine("includelib  \\masm32\\lib\\msvcrt.lib");
            sb.AppendLine();
            sb.AppendLine(".data");
            sb.AppendLine("msg db 'Return = %d',0");
            int count = 1;
            foreach (Variable var in routine.Functions[1].LocalVariables.FindAll(x => x.kind == Variable.Kind.Input && x.fake == true))
            {
                sb.AppendLine("f" + count + "  db 'Fake parameter #" + count + " ( " + var.fixedMin + " - " + var.fixedMax + " ):" +"',0");
                count++;
            }
            count = 1;
            foreach (Variable var in routine.Functions[1].LocalVariables.FindAll(x => x.kind == Variable.Kind.Input && x.fake == false))
            {
                sb.AppendLine("orig" + count + "  db 'Original parameter #" + count + ":" + "',0");
                count++;
            }
            sb.AppendLine("inf db '%d',0");
            sb.AppendLine();
            sb.AppendLine(".data?");
            sb.AppendLine("din dd ?");
            sb.AppendLine();
            sb.AppendLine(".code");
            sb.AppendLine();
            sb.AppendLine("start:");
            count = 1;
            foreach (Variable var in routine.Functions[1].LocalVariables.FindAll(x => x.kind == Variable.Kind.Input && x.fake == false))
            {
                sb.AppendLine("invoke  crt_printf,addr orig" + count);
                sb.AppendLine("invoke  crt_scanf,addr inf,addr din");
                sb.AppendLine("MOV eax, din");
                sb.AppendLine("PUSH eax");
                count++;
            }
            count = 1;
            foreach (Variable var in routine.Functions[1].LocalVariables.FindAll(x => x.kind == Variable.Kind.Input && x.fake == true))
            {
                if (Common.RandomPushValues == false)
                {
                    sb.AppendLine("invoke  crt_printf,addr  f" + count);
                    sb.AppendLine("invoke  crt_scanf,addr inf,addr din");
                    sb.AppendLine("MOV eax, din");
                    sb.AppendLine("PUSH eax");
                }
                else
                    sb.AppendLine("PUSH " + Randomizer.SingleNumber((int)var.fixedMin, (int)var.fixedMax)); // <- The automatized version: no questions, random fakeparams
                count++;
            }
            sb.AppendLine("CALL " + routine.Functions[1].globalID);
            sb.AppendLine("ADD esp, " + routine.Functions[1].LocalVariables.FindAll(x => x.kind == Variable.Kind.Input).Count * 4);
            sb.AppendLine("invoke  crt_printf,addr  msg,eax");
            sb.AppendLine("RET");
            sb.AppendLine();

            foreach (Function func in routine.Functions)
            {
                sb.AppendLine(GetAssemblyFromTAC(func));
            }

            sb.AppendLine();
            sb.AppendLine("end start");

            return sb.ToString();
        }

        /// <summary>
        /// Method to generate the assembly code from the TAC instructions.
        /// </summary>
        /// <param name="func">The function in TAC.</param>
        /// <returns>The assembly code.</returns>
        private static string GetAssemblyFromTAC(Function func)
        {
            StringBuilder sb = new StringBuilder();

            ReadableBBLabels.Clear();
            func.BasicBlocks.ForEach(x => ReadableBBLabels.Add(x.ID, string.Concat("LABEL_", counter++)));
            framestack = BuildStack(func);
            sb.AppendLine(Prologue(func));

            Obfuscator.Traversal.ReorderBasicBlocks(func);
            BasicBlock prev = func.BasicBlocks.First();
            foreach (BasicBlock bb in func.BasicBlocks)
            {
                /* We shouldn't write the "fake exit block". */
                if (bb.getSuccessors.Count == 0)
                    continue;

                // true ||
                if (bb.getPredecessors.Count() > 1 || (bb.getPredecessors.Count() == 1 &&
                        bb.getPredecessors.First() != prev))
                //|| (bb.getPredecessors.Count() == 1 &&
                //   bb.getPredecessors.First().Instructions.Last().statementType == 
                //   ExchangeFormat.StatementTypeType.EnumValues.eUnconditionalJump)
                {
                    sb.AppendLine(ReadableBBLabels[bb.ID] + ":");
                }

                foreach (Instruction inst in bb.Instructions)
                {
                    switch (inst.statementType)
                    {
                        case ExchangeFormat.StatementTypeType.EnumValues.eFullAssignment:
                            sb.Append(FullAssignment(inst, inst.polyRequired));
                            break;
                        case ExchangeFormat.StatementTypeType.EnumValues.eUnaryAssignment:
                            sb.Append(UnaryAssignment(inst, inst.polyRequired));
                            break;
                        case ExchangeFormat.StatementTypeType.EnumValues.eCopy:
                            sb.Append(Copy(inst, inst.polyRequired));
                            break;
                        case ExchangeFormat.StatementTypeType.EnumValues.eUnconditionalJump:
                            sb.Append(UnconditionalJump(inst, inst.polyRequired));
                            break;
                        case ExchangeFormat.StatementTypeType.EnumValues.eConditionalJump:
                            sb.Append(ConditionalJump(inst, inst.polyRequired));
                            break;
                        case ExchangeFormat.StatementTypeType.EnumValues.eProcedural:
                            sb.Append(Procedural(inst, inst.polyRequired));
                            break;
                        case ExchangeFormat.StatementTypeType.EnumValues.eIndexedAssignment:
                            break;
                        case ExchangeFormat.StatementTypeType.EnumValues.ePointerAssignment:
                            sb.Append(PointerAssignment(inst, inst.polyRequired));
                            break;
                        case ExchangeFormat.StatementTypeType.EnumValues.eNoOperation:
                            sb.Append(NoOperation(inst, inst.polyRequired));
                            break;
                        case ExchangeFormat.StatementTypeType.EnumValues.Invalid:
                            break;
                        default:
                            throw new AssemblerException("Statement type is not supported by Assembler.");
                    }
                    sb.AppendLine();
                }
                prev = bb;
            }

            sb.AppendLine(Epilogue(func));
            string optimizedAssembly = Optimize(sb.ToString());
            return optimizedAssembly;
        }

        private static string StackPointerOfVariable(Variable var)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("[ebp ");

            if (Offsets[var] >= 0)
                sb.Append("+ ");
            else
                sb.Append("- ");

            sb.Append(Math.Abs(Offsets[var]) + "]");

            return sb.ToString();
        }

        private static string NoOperation(Instruction inst, bool polyReq = false)
        {
            return "NOP";
        }

        private static string PointerAssignment(Instruction inst, bool polyReq = false)
        {
            StringBuilder sb = new StringBuilder();

            /* Parsing... */
            Variable leftvalue, right_var;
            int? right_const;
            Instruction.PoinerType operation;
            Parser.PointerAssignment(inst, out leftvalue, out right_var, out right_const, out operation);

            switch (operation)
            {
                case Instruction.PoinerType.Variable_EQ_AddressOfObject:
                    sb.AppendLine("LEA eax, " + StackPointerOfVariable(right_var));
                    sb.AppendLine("MOV " + StackPointerOfVariable(leftvalue) + ", eax");
                    break;
                case Instruction.PoinerType.Variable_EQ_PointedObject:
                    sb.AppendLine("MOV eax, " + StackPointerOfVariable(right_var));
                    sb.AppendLine("MOV ebx, [eax]");
                    sb.AppendLine("MOV " + StackPointerOfVariable(leftvalue) + ", ebx");
                    break;
                case Instruction.PoinerType.PointedObject_EQ_Variable:
                    sb.AppendLine("MOV eax, " + StackPointerOfVariable(leftvalue));
                    sb.AppendLine("MOV ebx, " + StackPointerOfVariable(right_var));
                    sb.AppendLine("MOV [eax], ebx");
                    break;
                case Instruction.PoinerType.PointedObject_EQ_Number:
                    sb.AppendLine("MOV eax, " + StackPointerOfVariable(leftvalue));
                    sb.AppendLine("MOV ebx, " + right_const);
                    sb.AppendLine("MOV [eax], ebx");
                    break;
            }

            return sb.ToString();
        }

        private static string ConditionalJump(Instruction inst, bool polyReq = false)
        {
            StringBuilder sb = new StringBuilder();

            /* We parse the conditional jump. */
            Variable left, right_var;
            int? right_const;
            Instruction.RelationalOperationType relop;
            BasicBlock truebb, falsebb;
            Parser.ConditionalJump(inst, out left, out right_var, out right_const, out relop, out truebb, out falsebb); 
            
            /* Now we build the assembly instructions. */
            sb.AppendLine("MOV eax, " + StackPointerOfVariable(left));
            if (right_var != null && right_const == null)
            {
                sb.AppendLine("MOV ebx, " + StackPointerOfVariable(right_var));
                sb.AppendLine("CMP eax, ebx");
            }
            else
                sb.AppendLine("CMP eax, " + right_const);
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
            sb.AppendLine(ReadableBBLabels[truebb.ID]);

            return sb.ToString();
        }

        private static string UnconditionalJump(Instruction inst, bool polyReq = false)
        {
            StringBuilder sb = new StringBuilder();

            BasicBlock jumptarget;
            Parser.UnconditionalJump(inst, out jumptarget);

            sb.AppendLine("JMP " + ReadableBBLabels[jumptarget.ID]);

            return sb.ToString();
        }

        private static string Copy(Instruction inst, bool polyReq = false)
        {
            StringBuilder sb = new StringBuilder();

            /* Parsing... */
            Variable leftvalue, right_var;
            int? right_const;
            Parser.Copy(inst, out leftvalue, out right_var, out right_const);

            /* We copy a variable's value to another. */
            if (right_var != null && right_const == null)
            {
                sb.AppendLine("MOV eax, " + StackPointerOfVariable(right_var));
                sb.AppendLine("MOV " + StackPointerOfVariable(leftvalue) + ", eax");
            }

            /* We copy a constant value to a variable. */
            else if (right_var == null && right_const != null)
            {
                sb.AppendLine("MOV eax, " + right_const);
                sb.AppendLine("MOV " + StackPointerOfVariable(leftvalue) + ", eax");
            }

            else
                throw new ObfuscatorException("Not valid copy instruction: both right variable and constant.");

            return sb.ToString();
        }

        private static string UnaryAssignment(Instruction inst, bool polyReq = false)
        {
            StringBuilder sb = new StringBuilder();

            /* Parsing... */
            Variable leftvalue, rightvalue;
            Instruction.UnaryOperationType op;
            Parser.UnaryAssignment(inst, out leftvalue, out rightvalue, out op);

            /* Building the assembly... */
            sb.AppendLine("MOV eax, " + StackPointerOfVariable(rightvalue));

            if (op == Instruction.UnaryOperationType.ArithmeticNegation)
                sb.AppendLine("NEG eax");

            else
                sb.AppendLine("NOT eax");

            sb.AppendLine("MOV " + StackPointerOfVariable(leftvalue) + ", eax");

            return sb.ToString();
        }

        private static string FullAssignment(Instruction inst, bool polyReq = false)
        {
            StringBuilder sb = new StringBuilder();

            /* Parsing... */
            Variable leftvalue, right1, right2_var;
            int? right2_const;
            Instruction.ArithmeticOperationType op;
            Parser.FullAssignment(inst, out leftvalue, out right1, out right2_var, out right2_const, out op);

            /* Building the assembly... */
            sb.AppendLine("MOV eax, " + StackPointerOfVariable(right1));

            if (right2_var != null && right2_const == null)
                sb.AppendLine("MOV ebx, " + StackPointerOfVariable(right2_var));

            if (op == Instruction.ArithmeticOperationType.Addition || op == Instruction.ArithmeticOperationType.Subtraction)
            {
                if (op == Instruction.ArithmeticOperationType.Addition)
                    sb.Append("ADD ");
                else
                    sb.Append("SUB ");

                sb.Append("eax, ");

                if (right2_var != null && right2_const == null)
                    sb.Append("ebx");
                else
                    sb.Append(right2_const);

                sb.AppendLine();
            }

            else
            {
                /* We check if there is a constant value that is a power of 2. */
                bool shift_available = false;
                if (right2_const != null && right2_var == null)
                {
                    int pow = Convert.ToInt32(Math.Log((int)right2_const, 2));
                    int tmp = Convert.ToInt32(Math.Pow(2, pow));
                    shift_available = (tmp == right2_const);
                    
                    if (shift_available)
                    {
                        /* It is, so we can use shifts instead of multiplication and division. */
                        if (op == Instruction.ArithmeticOperationType.Multiplication)
                            sb.Append("SAL eax, ");
                        else
                            sb.Append("SAR eax, ");

                        sb.AppendLine(pow.ToString());
                    }
                }

                /* 
                 * We are not using a contant which is a power of 2, or we are not using
                 * a constant at all, so we have to use multiplication or division.
                 */
                if (!shift_available)
                {
                    if (op == Instruction.ArithmeticOperationType.Multiplication)
                        sb.Append("MUL ");
                    else
                        sb.Append("DIV ");

                    if (right2_var != null && right2_const == null)
                        sb.Append("ebx");
                    else
                        sb.Append(right2_const);

                    sb.AppendLine();
                }
            }

            sb.AppendLine("MOV " + StackPointerOfVariable(leftvalue) + ", eax");

            return sb.ToString();
        }

        private static string Procedural(Instruction inst, bool polyReq = false)
        {
            StringBuilder sb = new StringBuilder();

            /* Parsing... */
            Variable var;
            int? num;
            Instruction.ProceduralType type;
            Function called_func;
            Parser.Procedural(inst, out var, out num, out type, out called_func);

            /* Building assembly... */
            switch (type)
            {
                case Instruction.ProceduralType.Call:
                    if (called_func == null)
                        sb.AppendLine(";CALL " + inst.TACtext.Split(' ')[1]);
                    else
                        sb.AppendLine("CALL " + called_func.globalID);
                    break;
                case Instruction.ProceduralType.Param:
                    sb.Append("PUSH ");
                    if (var != null && num == null)
                        sb.Append(StackPointerOfVariable(var));
                    else
                        sb.Append(num);
                    sb.AppendLine();

                    if (var != null)
                        framestack -= var.memoryRegionSize;
                    else
                        framestack -= 4;

                    break;
                case Instruction.ProceduralType.Retrieve:
                    sb.AppendLine("MOV " + StackPointerOfVariable(var) + ", eax");
                    break;
                case Instruction.ProceduralType.Return:
                    if (var != null || num != null)
                    {
                        sb.Append("MOV eax, ");
                        if (var != null && num == null)
                            sb.Append(StackPointerOfVariable(var));
                        else
                            sb.Append(num);
                        sb.AppendLine();
                    }
                    sb.Append(ReturnFromFunction(inst.parent.parent));
                    break;
            }

            return sb.ToString();
        }

        private static string Prologue(Function func)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(func.globalID + " proc");
            sb.AppendLine("PUSH ebp");
            sb.AppendLine("MOV ebp, esp");
            sb.AppendLine("SUB esp, " + Math.Abs(framestack + 4));
            return sb.ToString();
        }

        private static string Epilogue(Function func)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(func.globalID + " endp");

            return sb.ToString();
        }

        private static int BuildStack(Function func)
        {
            int fs = -4, ps = 8;
            foreach (Variable var in func.LocalVariables.FindAll(x => x.kind != Variable.Kind.Input))
            {
                Offsets.Add(var, fs);
                fs -= var.memoryRegionSize;
            }
            func.LocalVariables.Reverse();
            foreach (Variable var in func.LocalVariables.FindAll(x => x.kind == Variable.Kind.Input))
            {
                Offsets.Add(var, ps);
                ps += var.memoryRegionSize;
            }
            return fs;
        }

        private static string ReturnFromFunction(Function func)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ADD esp, " + Math.Abs(framestack + 4));
            sb.AppendLine("POP ebp");
            sb.AppendLine("RET");
            return sb.ToString();
        }


        private static string Optimize(string funcASM)
        {
            System.Collections.Specialized.StringCollection movmov = new System.Collections.Specialized.StringCollection();
            funcASM = funcASM.Replace("\r\n\r\n", "\r\n");
            Match matchResult = Regex.Match(funcASM, @"mov (.*), (.*)\r\nmov (\2), (\1)", RegexOptions.IgnoreCase);
            while (matchResult.Success)
            {
                movmov.Add(matchResult.Value);
                matchResult = matchResult.NextMatch();
            }
            foreach (string item in movmov)
                funcASM = funcASM.Replace(item, string.Empty);
            return funcASM;
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

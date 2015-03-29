using ExchangeFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Services;

namespace Platform_x64
{
    public static class PseudoCode_x64
    {
        private static RoutineType routine;
        private static Variables currentFunctionLocalVars;
        private static string currentFunctionReturnType;
        //Stores the pseudocode
        private static string[] original;
        private static Random randNumbers = new Random(DateTime.Now.Millisecond);
        private static List<int> embeddedBasicBlocks = new List<int>();
        private static bool functionsHasDivisionModulo;

        /// <summary>
        /// Translates PseudoCode (PC) to Three Address Code (TAC)
        /// </summary>
        /// <param name="path2PC">Path to the PC file</param>
        /// <returns>A XML document</returns>
        public static XmlDocument GetTAC(string path2PC)
        {
            XmlDocument doc = new XmlDocument();
            ExchangeFormat.Exchange exchange = new Exchange(doc);
            routine = exchange.Routine.Append();
            routine.Description.Value = System.IO.Path.GetFileNameWithoutExtension(path2PC);
            original = System.IO.File.ReadAllLines(path2PC);
            PreProc();
            Translate();
            //For the last function
            FakeReturnInstruction();
            return doc;

        }
        /// <summary>
        /// Preprocesses the PC removing unnecessary elements
        /// </summary>
        private static void PreProc()
        {
            string[] toReplacePC = { "_cdecl ",
                                       "*(_BYTE *)",
                                       "*(_DWORD *)",
                                       ";", "signed ",
                                       "unsigned ",
                                       "(signed ",
                                       "(unsigned ", 
                                       "__int64 ",
                                       "__fastcall ",
                                       "_main(int argc, const char **argv, const char **envp)",
                                       "(unint)",
                                       "unint" }; //Array of unnecessary elements
            string[] toReplaceTAC = { "", "", "", "", "", "", "(", "(", "int ", "", "sub_123()", "", "int" };
            for (int i = 0; i < original.Length; i++)
            {
                //Replacing unnecessary elements
                for (int j = 0; j < toReplacePC.Length; j++)
                {
                    if (original[i].Contains(toReplacePC[j]))
                        if (!original[i].Contains("for"))
                            original[i] = original[i].Replace(toReplacePC[j], toReplaceTAC[j]);
                }
                //Removing comments
                if (original[i].Contains("//"))
                    original[i] = original[i].Remove(original[i].IndexOf("//"));
                original[i] = original[i].TrimStart();
                original[i] = original[i].TrimEnd();
            }
        }
        /// <summary>
        /// Goes through the lines of the PC calling the necessary functions to translate
        /// the code to TAC
        /// </summary>
        private static void Translate()
        {
            string[] types = { "int ", "char ", "long ", "short ", "int* ", "char* ", "long* ", "short* " };
            int i = 0;
            while (i < original.Length)
            {
                bool translated = false;
                if ((!original[i].Equals("{")) && (!original[i].Equals("}")) && (original[i].Length > 0))
                {
                    //Searching for types within each line                    
                    foreach (string type in types)
                    {
                        if (original[i].Contains(type))
                        {
                            //For functions
                            if (original[i].Contains("sub_"))
                            {
                                CreateFunction(original[i]);
                                translated = true;
                                i++;
                                break;
                            }
                            //For variables
                            else
                            {
                                CreateLocalVariable(original[i]);
                                translated = true;
                                i++;
                                break;
                            }
                        }
                    }
                    //For instructions
                    if (translated == false)
                        //The index will be updated accordingly to the number of lines consumed by the instruction
                        i = CreateInstruction(i);
                }
                else
                    i++;
            }
        }
        /// <summary>
        /// Creates a new function
        /// </summary>
        /// <param name="line">PC line, similar to "int sub_401334(int a1, int a2)"</param>
        private static void CreateFunction(string line)
        {
            if (routine.Function.Count > 0)
                FakeReturnInstruction();
            //Extracting the returning type
            currentFunctionReturnType = line.Substring(0, line.IndexOf(" "));
            //Changing the line to "sub_401334(int a1 int a2)"
            line = line.Substring(line.IndexOf("sub_"));
            //Removing the parameters
            string aux = line.Remove(line.IndexOf('('));
            FunctionType newFunction = routine.Function.Append();
            newFunction.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newFunction.GlobalID.Value = aux;
            newFunction.CalledFrom.EnumerationValue = CalledFromType.EnumValues.eExternalOnly;
            currentFunctionLocalVars = newFunction.Local.Append();

            // PC line referring to non-main functions
            if (!line.Contains("()"))
            {
                //Removing commas
                line = line.Replace(",", "");
                //Separating parameters
                string[] tokens = line.Split('(');
                aux = tokens[1].Replace(")", "");
                //Creating variables for the parameters
                CreateLocalVariable(aux);
                tokens = aux.Split(' ');
                string inputVars = string.Empty;
                //Creating string of Input Variables
                for (int i = 1; i < tokens.Length; i += 2)
                {
                    if (inputVars.Length != 0)
                        inputVars = string.Join(" ", inputVars, GetIDVariable(tokens[i]));
                    else
                        inputVars = GetIDVariable(tokens[i]);
                }
                routine.Function[routine.Function.Count - 1].RefInputVars.Value = inputVars;
            }
            BasicBlockType newBasicBlock = newFunction.BasicBlock.Append();
            newBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            functionsHasDivisionModulo = false;
        }
        /// <summary>
        /// Creates a new local variable if it does not exist already        
        /// </summary>
        /// <param name="line">Line similar to "int a int b int c"</param>
        private static void CreateLocalVariable(string line)
        {
            //The parameter line is expected to have a content similar to "int a int b int c"
            bool pointer;
            string[] tokens = line.Split(' ');
            VariableType newVariable;
            for (int i = 0; i < tokens.Length - 1; i += 2)
            {
                //Checking whether we already have created this variable
                if (GetIDVariable(tokens[i + 1]).Equals(tokens[i + 1]))
                {
                    //Checking wheter it is a pointer
                    if (tokens[i + 1].IndexOf('*') != -1)
                    {
                        pointer = true;
                        tokens[i + 1] = tokens[i + 1].Remove(tokens[i + 1].IndexOf('*'), 1);
                    }
                    else
                        pointer = false;
                    newVariable = currentFunctionLocalVars.Variable.Append();
                    newVariable.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
                    newVariable.Value = string.Concat("v_", newVariable.ID.Value);
                    newVariable.GlobalID.Value = tokens[i + 1];
                    newVariable.Pointer.Value = pointer;
                    if (tokens[i].Equals("char"))
                        newVariable.MemoryRegionSize.Value = 1;
                    else
                        newVariable.MemoryRegionSize.Value = 4;
                }
            }
        }
        /// <summary>
        /// Selects the necessary function to create a new instruction
        /// </summary>
        /// <param name="lineIndex">Index of the PC line in the array "original" </param>
        /// <returns>The correct line index after the creation of the instruction</returns>
        private static int CreateInstruction(int lineIndex, bool innerScope = false)
        {
            if (original[lineIndex].Contains("if"))
            {
                lineIndex = IfInstruction(lineIndex, innerScope);
            }
            else if (original[lineIndex].Contains("for"))
            {
                lineIndex = ForInstruction(lineIndex, innerScope);
            }
            else if (original[lineIndex].Contains("do"))
            {
                lineIndex = DoWhileInstruction(lineIndex, innerScope);
            }
            else if (original[lineIndex].Contains("while"))
            {
                lineIndex = WhileInstruction(lineIndex, innerScope);
            }
            else if (original[lineIndex].Contains("return"))
            {
                PreReturn(lineIndex);
                lineIndex++;
            }
            else if ((original[lineIndex].IndexOf("sub_") != -1 && original[lineIndex].IndexOf("=") == -1)
                || original[lineIndex].IndexOf("scanf") != -1 || original[lineIndex].IndexOf("printf") != -1)
            {
                CallInstruction(original[lineIndex]);
                lineIndex++;
            }
            else if (original[lineIndex].IndexOf('=') != -1 || original[lineIndex].IndexOf("++") != -1 || original[lineIndex].IndexOf("--") != -1)
            {
                PreFullAssignCopy(lineIndex);
                lineIndex++;
            }
            else
                Console.WriteLine("Instruction not implemented: " + original[lineIndex]);
            return lineIndex;
        }
        /// <summary>
        /// Preprocesses the line before creating the return instruction
        /// </summary>
        /// <param name="lineIndex">Index of the PC line in the array "original" </param>
        private static void PreReturn(int lineIndex)
        {
            string aux = original[lineIndex];
            //aux is something like "return sub_401334(a)"
            if (aux.Contains("sub_"))
            {
                aux = aux.Substring(aux.IndexOf("sub_")); //sub_401334(a)
                aux = aux.Remove(aux.IndexOf('(')); //sub_401334
                CallInstruction(original[lineIndex].Substring(original[lineIndex].IndexOf("sub_")));
                ReturnInstruction(string.Concat("return ", aux));
            }
            //aux is something like "return a1 + v5"
            else if (aux.Contains(" + ") || aux.Contains(" - ") || aux.Contains(" * ") || aux.Contains(" / ") || aux.Contains(" % "))
                ReturnInstruction(string.Concat("return ", ProcessArithmeticOperations(aux.Substring(aux.IndexOf(' ')))));
            //aux is something like "return (a1 + v5)"
            else if (aux.Contains(" + ") || aux.Contains(" - ") || aux.Contains(" * ") || aux.Contains(" / ") || aux.Contains(" % ") && aux.Contains("(") && aux.Contains(")"))
            {
                //Removing the especial caracters
                aux.Replace("(", "");
                aux.Replace(")", "");
                ReturnInstruction(string.Concat("return ", ProcessArithmeticOperations(aux.Substring(aux.IndexOf(' ')))));
                
            }
            //aux is something like "return 0" or "return a1"
            else
                ReturnInstruction(aux);
        }
        /// <summary>
        /// Creates a return instruction
        /// </summary>
        /// <param name="line">PC line, similar to "return v3"</param>
        private static void ReturnInstruction(string line)
        {
            if (line.Contains("*"))
                line = line.Remove(line.IndexOf('*'), 1);
            string[] tokens = line.Split(' ');
            int indexFunction = routine.Function.Count - 1;
            int indexBasicblock = routine.Function[indexFunction].BasicBlock.Count - 1;

            if (tokens.Length > 1 && functionsHasDivisionModulo)
            {
                BasicBlockType returnBasicBlock = routine.Function[indexFunction].BasicBlock.Append();
                returnBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
                int returnBasicBlockIndex = routine.Function[indexFunction].BasicBlock.Count - 1;
                LinkPredecessor(indexFunction, returnBasicBlockIndex, indexBasicblock);
                UncondJumpInstruction(indexBasicblock, returnBasicBlock.ID.Value);
                indexBasicblock = returnBasicBlockIndex;
            }

            InstructionType newInstruction = routine.Function[indexFunction].BasicBlock[indexBasicblock].Instruction.Append();
            newInstruction.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newInstruction.StatementType.EnumerationValue = StatementTypeType.EnumValues.eProcedural;
            //Real return
            if (tokens.Length > 1)
            {
                if (!GetIDVariable(tokens[1]).Equals(tokens[1]))
                    newInstruction.RefVars.Value = GetIDVariable(tokens[1]); //Adding "v3" to RefVars
                newInstruction.Value = string.Concat("return ", GetValueVariable(tokens[1]));
                routine.Function[indexFunction].RefOutputVars.Value = GetIDVariable(tokens[1]);
            }
            //Fake return
            else
                newInstruction.Value = "return";
        }
        /// <summary>
        /// Creates the fake exit block
        /// </summary>
        private static void FakeReturnInstruction()
        {
            int functionIndex = routine.Function.Count - 1;
            int basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            BasicBlockType newBasicBlock = routine.Function[functionIndex].BasicBlock.Append();
            newBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = newBasicBlock.ID.Value;
            ReturnInstruction("return");
        }
        /// <summary>
        /// Creates a call instruction
        /// </summary>
        /// <param name="line">PC line, similar to "sub_401334(a, b)"</param>
        /// <param name="returnType">Return type in case the currentFunctionReturnType should not be used"</param>
        private static void CallInstruction(string line, string returnType = "")
        {
            int numParams = 0;
            int indexFunction = routine.Function.Count - 1;
            int indexBasicblock = routine.Function[indexFunction].BasicBlock.Count - 1;
            //Removing mask in case of printf or scanf
            string type = string.Empty;
            if (line.Contains("%d"))
            {
                line = line.Replace("\"%d\", ", "");
                type = "_int";
            }
            else if (line.Contains("%c"))
            {
                line = line.Replace("\"%c\", ", "");
                type = "_char";
            }
            // x64 cases
            else if (line.Contains("Format"))
            {
                line = line.Replace("Format, ", "");
                type = "_int";
            }
            else if (line.Contains("_0"))
            {
                line = line.Replace("aD_0, ", "");
                type = "_int";
            }
            else if (line.Contains("aD"))
            {
                line = line.Replace("aD, ", "");
                type = "_int";
            }
            //Scanf case with 3 parameters
            if (line.Contains("envp"))
            {
                line = line.Replace(", envp", "");
                type = "_int";
            }
            //Removing address sign in case of scanf
            if (line.Contains("&"))
                line = line.Replace("&", "");
            InstructionType newInstruction;
            //Dealing with parameters
            if (!line.Contains("()"))
            {
                //Extracting parameters
                string aux = line.Substring(line.IndexOf('(') + 1);
                aux = aux.Replace(")", "");
                //Checking whether we have operations inside the parameters list. Something like "sub_4114C0(v10, v10 + 10)"
                string[] tokens = aux.Split(',');
                foreach (string token in tokens)
                {
                    if (token.Contains("+") || token.Contains("-") || token.Contains("*") || token.Contains("/") || token.Contains("%"))
                        aux = aux.Replace(token, string.Concat(" ", ProcessArithmeticOperations(token)));
                }
                //Removing commas (if they exist)
                aux = aux.Replace(",", "");
                //Creating "param" instructions
                tokens = aux.Split(' ');
                foreach (string token in tokens)
                {
                    newInstruction = routine.Function[indexFunction].BasicBlock[indexBasicblock].Instruction.Append();
                    newInstruction.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
                    newInstruction.StatementType.EnumerationValue = StatementTypeType.EnumValues.eProcedural;
                    newInstruction.RefVars.Value = GetIDVariable(token);
                    newInstruction.Value = string.Concat("param ", GetValueVariable(token));
                    numParams++;
                }
            }
            string[] tokens2 = line.Split('(');
            //Creating the variable to retrieve the returned value
            if (returnType.Length == 0)
                returnType = currentFunctionReturnType;
            CreateLocalVariable(string.Join(" ", returnType, tokens2[0]));
            //Creating the call instruction
            newInstruction = routine.Function[indexFunction].BasicBlock[indexBasicblock].Instruction.Append();
            newInstruction.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newInstruction.StatementType.EnumerationValue = StatementTypeType.EnumValues.eProcedural;
            newInstruction.Value = string.Join(" ", "call", string.Concat(GetIDFunction(tokens2[0]), type), numParams);
            for (int i = 0; i < routine.Function.Count; i++)
            {
                if (routine.Function[i].GlobalID.Value == tokens2[0])
                    routine.Function[i].CalledFrom.EnumerationValue = CalledFromType.EnumValues.eBoth;
            }
            //Creating the retrieve instruction
            //If the called function is a "scanf", we pass only the variable
            if (line.Contains("scanf"))
            {
                string aux = line.Substring(line.IndexOf('(') + 1);
                aux = aux.Replace(")", "");
                RetrieveInstruction(aux);
            }
            //If the instruction is not a "scanf" nor a "printf", then we create a retrieve instruction using the whole line
            else if (!line.Contains("printf"))
                RetrieveInstruction(line);
        }
        /// <summary>
        /// Creates a retrieve instruction
        /// </summary>
        /// <param name="line">PC line, similar to "sub_401334(a, b)"</param>
        private static void RetrieveInstruction(string line)
        {
            string[] tokens = line.Split('(');
            int indexFunction = routine.Function.Count - 1;
            int indexBasicblock = routine.Function[indexFunction].BasicBlock.Count - 1;
            InstructionType newInstruction = newInstruction = routine.Function[indexFunction].BasicBlock[indexBasicblock].Instruction.Append();
            newInstruction.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newInstruction.StatementType.EnumerationValue = StatementTypeType.EnumValues.eProcedural;
            newInstruction.RefVars.Value = GetIDVariable(tokens[0]);
            newInstruction.Value = string.Concat("retrieve ", GetValueVariable(tokens[0]));
        }
        /// <summary>
        /// Preprocesses the line before creating a "full assignment" or a "copy" instruction
        /// </summary>
        /// <param name="lineIndex">Index of the PC line in the array "original" </param>
        /// <param name="operation">Customized operation, not explicit in PC</param>
        private static void PreFullAssignCopy(int lineIndex, string operation = "")
        {
            string aux = operation;
            if (aux.Length == 0)
                aux = original[lineIndex];
            if (aux.Contains("/") || aux.Contains("%"))
                functionsHasDivisionModulo = true;
            if (aux.Contains("sub_"))
                CopyInstruction(aux);
            else
            {
                string[] tokens = aux.Split(' ');
                if (tokens.Length == 1)
                {
                    //aux is something like "v5++"
                    if (aux.IndexOf("++") != -1)
                    {
                        aux = aux.Replace("++", "");
                        FullAssignInstruction(string.Join(" ", aux, "=", aux, "+ 1"));
                    }
                    //aux is something like "v5--"
                    else
                    {
                        aux = aux.Replace("--", "");
                        FullAssignInstruction(string.Join(" ", aux, "=", aux, "- 1"));
                    }
                }
                else if (tokens.Length == 3)
                {
                    //aux is something like "v5 += v1"
                    if (aux.Contains("+=") || aux.Contains("-=") || aux.Contains("*=") || aux.Contains("/="))
                    {
                        tokens[1] = tokens[1].Remove(tokens[1].IndexOf('='), 1);
                        FullAssignInstruction(string.Join(" ", tokens[0], "=", tokens[0], tokens[1], tokens[2]));
                    }
                    //aux is "v5 = v1"
                    else
                        CopyInstruction(aux);
                }
                else
                {
                    //aux is something like "a1 *= i - 1 +..."
                    if (aux.Contains("+=") || aux.Contains("-=") || aux.Contains("*=") || aux.Contains("/="))
                    {
                        tokens[1] = tokens[1].Remove(tokens[1].IndexOf('='), 1);
                        FullAssignInstruction(string.Join(" ", tokens[0], "=", ProcessArithmeticOperations(aux.Substring(aux.IndexOf('=') + 1)), tokens[1], tokens[0]));
                    }
                    else
                    {
                        //aux is "v5 = v1 + v2 -..."
                        CopyInstruction(string.Join(" ", tokens[0], "=", ProcessArithmeticOperations(aux.Substring(aux.IndexOf('=') + 1))));
                    }
                }
            }
        }
        /// <summary>
        /// Creates a "full assignment" instruction
        /// </summary>
        /// <param name="line">PC line, similar to "v3 = a2 * a1"</param>
        private static void FullAssignInstruction(string line)
        {
            string[] tokens = line.Split(' ');
            int indexFunction = routine.Function.Count - 1;
            int indexBasicblock = routine.Function[indexFunction].BasicBlock.Count - 1;
            InstructionType newInstruction = routine.Function[indexFunction].BasicBlock[indexBasicblock].Instruction.Append();
            newInstruction.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newInstruction.StatementType.EnumerationValue = StatementTypeType.EnumValues.eFullAssignment;

            //Dealing with pointers
            string[] forPointers = { string.Empty, string.Empty, string.Empty };
            tokens[0] = DealWithPointers(tokens[0], out forPointers[0]);
            tokens[2] = DealWithPointers(tokens[2], out forPointers[1]);
            tokens[4] = DealWithPointers(tokens[4], out forPointers[2]);

            string refVars = string.Empty;
            //Adding v3
            if (!GetIDVariable(tokens[0]).Equals(tokens[0]))
                refVars = GetIDVariable(tokens[0]);
            //Adding a2
            if (!GetIDVariable(tokens[2]).Equals(tokens[2]))
                if (refVars.Length != 0)
                    refVars = string.Join(" ", refVars, GetIDVariable(tokens[2]));
                else
                    refVars = GetIDVariable(tokens[2]);
            //Adding a1
            if (!GetIDVariable(tokens[4]).Equals(tokens[4]))
                if (refVars.Length != 0)
                    refVars = string.Join(" ", refVars, GetIDVariable(tokens[4]));
                else
                    refVars = GetIDVariable(tokens[4]);
            newInstruction.RefVars.Value = refVars;
            newInstruction.Value = string.Join("", forPointers[0], GetValueVariable(tokens[0]), " := ",
                forPointers[1], GetValueVariable(tokens[2]), " ", tokens[3], " ", forPointers[2], GetValueVariable(tokens[4]));
        }
        /// <summary>
        /// Creates a "copy" instruction
        /// </summary>
        /// <param name="line">PC line, similar to "v3 = 10" or "v3 = a1" or "v5 = sub_4113E0(v6)"</param>
        private static void CopyInstruction(string line)
        {
            string[] tokens;
            //Dealing with calls, something like "v5 = sub_4113E0(v6)"
            if (line.Contains("sub_"))
            {
                //Extracting only the call "sub_4113E0(v6)"
                tokens = line.Split('=');
                tokens[0] = tokens[0].TrimEnd();
                tokens[1] = tokens[1].TrimStart();
                string returnType = string.Empty;
                if (GetMemoryRegionSizeVariable(tokens[0]) == 4)
                    returnType = "int";
                else
                    returnType = "char";
                CallInstruction(tokens[1], returnType);
                //Replacing the instruction to something like "v5 = sub_4113E0", where "sub_4113E0" is a variable
                string aux = tokens[1].Remove(tokens[1].IndexOf('(')); //sub_4113E0
                line = string.Join(" ", tokens[0], "=", aux);
            }

            tokens = line.Split(' ');

            int functionIndex = routine.Function.Count - 1;
            int basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            InstructionType newInstruction = routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction.Append();
            newInstruction.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            if (line.IndexOf('*') != -1 || line.IndexOf('&') != -1)
                newInstruction.StatementType.EnumerationValue = StatementTypeType.EnumValues.ePointerAssignment;
            else
                newInstruction.StatementType.EnumerationValue = StatementTypeType.EnumValues.eCopy;

            //Dealing with pointers
            string[] forPointers = { string.Empty, string.Empty };
            tokens[0] = DealWithPointers(tokens[0], out forPointers[0]);
            tokens[2] = DealWithPointers(tokens[2], out forPointers[1]);

            string refVars = string.Empty;
            if (!GetIDVariable(tokens[0]).Equals(tokens[0]))
                refVars = GetIDVariable(tokens[0]);
            if (!GetIDVariable(tokens[2]).Equals(tokens[2]))
                if (refVars.Length != 0)
                    refVars = string.Join(" ", refVars, GetIDVariable(tokens[2]));
                else
                    refVars = GetIDVariable(tokens[2]);
            newInstruction.RefVars.Value = refVars;
            newInstruction.Value = string.Join("", forPointers[0], GetValueVariable(tokens[0]), " := ", forPointers[1],
                GetValueVariable(tokens[2]));
        }
        /// <summary>
        /// Creates a "if" instruction
        /// </summary>
        /// <param name="lineIndex">Index of the PC line in the array "original" </param>
        /// <returns>The correct line index after processing the inner scope of the instruction</returns>
        private static int IfInstruction(int lineIndex, bool innerScope)
        {
            int functionIndex = routine.Function.Count - 1;
            int condJumpBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            int linkedLogicalBasicBlockIndex = -1;
            //Stores the index of the basic blocks that need linking after logical operations and to which branch they belong
            Dictionary<int, bool> logicalBasicBlocks = new Dictionary<int, bool>();

            //Extracting the control elements
            string aux = original[lineIndex].Substring(original[lineIndex].IndexOf('(') + 2);
            aux = aux.Remove(aux.Length - 2);
            //Checking whether we have logical operations in the condition
            if (aux.Contains("||") || aux.Contains("&&"))
            {
                logicalBasicBlocks = ProcessLogicalOperations(aux);
                //Selecting the right predecessor for the true lane based in the logical operations
                if (logicalBasicBlocks.Last().Value == true)
                {
                    condJumpBasicBlockIndex = logicalBasicBlocks.Last().Key;
                    linkedLogicalBasicBlockIndex = condJumpBasicBlockIndex;
                    logicalBasicBlocks.Remove(linkedLogicalBasicBlockIndex);
                }
                else
                {
                    linkedLogicalBasicBlockIndex = condJumpBasicBlockIndex;
                    logicalBasicBlocks.Remove(linkedLogicalBasicBlockIndex);
                }
            }
            else
                PreCondJump(aux);

            //New Basic Block for the True Lane
            BasicBlockType trueBasicBlock = routine.Function[functionIndex].BasicBlock.Append();
            int firstTrueBasickBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            trueBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            LinkPredecessor(functionIndex, firstTrueBasickBlockIndex, condJumpBasicBlockIndex, true);
            CompleteCondJump(functionIndex, condJumpBasicBlockIndex, trueBasicBlock.ID.Value);
            //Processing instructions between { }        
            lineIndex = ProcessInnerScope(lineIndex);
            int lastTrueBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;

            int lastFalseBasicBlockIndex = -1;
            if (original[lineIndex].Contains("else"))
            {
                //New Basic Block for the False Lane
                BasicBlockType falseBasicBlock = routine.Function[functionIndex].BasicBlock.Append();
                falseBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
                int firsFalseBasickBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
                //Linking the basic blocks created by logical operations (if they exist)
                if (linkedLogicalBasicBlockIndex != -1)
                    LinkLogicalBasicBlocks(logicalBasicBlocks, firstTrueBasickBlockIndex, firsFalseBasickBlockIndex);
                else
                    LinkPredecessor(functionIndex, firsFalseBasickBlockIndex, condJumpBasicBlockIndex);
                //Processing instructions between { }
                lineIndex = ProcessInnerScope(lineIndex);
                lastFalseBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            }
            //If we don't have a false lane and we are in an inner scope, then we need an auxiliar basic block to be able to jump
            //to the correct place in case the conditional jump is false
            else if (innerScope && original[lineIndex].Contains("}"))
            {
                BasicBlockType auxInnerBasicBlock = routine.Function[functionIndex].BasicBlock.Append();
                auxInnerBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
                int auxInnerBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
                //Linking the basic blocks created by logical operations (if they exist)
                if (linkedLogicalBasicBlockIndex != -1)
                    LinkLogicalBasicBlocks(logicalBasicBlocks, firstTrueBasickBlockIndex, auxInnerBasicBlockIndex);
                else
                    LinkPredecessor(functionIndex, auxInnerBasicBlockIndex, condJumpBasicBlockIndex);
                lastFalseBasicBlockIndex = auxInnerBasicBlockIndex;
            }

            //Checking whether we have instructions after the current IF or the end of another scope
            if (!original[lineIndex].Contains("}"))
            {
                //New Basic Block for the intructions after the IF
                BasicBlockType newBasicBlock = routine.Function[functionIndex].BasicBlock.Append();
                newBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
                int newBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;

                //Linking the new basic block to the false lane (if it exists)
                if (lastFalseBasicBlockIndex != -1)
                {
                    LinkPredecessor(functionIndex, newBasicBlockIndex, lastFalseBasicBlockIndex);
                    UncondJumpInstruction(lastFalseBasicBlockIndex, newBasicBlock.ID.Value);
                }
                //Linking the new basic block to the true lane
                LinkPredecessor(functionIndex, newBasicBlockIndex, lastTrueBasicBlockIndex);

                //Linking the new basic block to the conditional jump
                //If we have logical operations, then we have to link all the logical basic blocks
                if (linkedLogicalBasicBlockIndex != -1)
                    LinkLogicalBasicBlocks(logicalBasicBlocks, firstTrueBasickBlockIndex, newBasicBlockIndex);
                //If we don't have "else", then we link the conditional jump to the new basic block
                else if (lastFalseBasicBlockIndex == -1)
                    LinkPredecessor(functionIndex, newBasicBlockIndex, condJumpBasicBlockIndex);

                //Linking embedded basic blocks (if they exist)
                foreach (int basicBlockIndex in embeddedBasicBlocks)
                {
                    LinkPredecessor(functionIndex, newBasicBlockIndex, basicBlockIndex);
                    //If we don't have an unconditional jump to new basic block, we have to create it
                    if (!routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction.Exists ||
                        (routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction.Exists &&
                        routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction.Last.StatementType.EnumerationValue !=
                        StatementTypeType.EnumValues.eUnconditionalJump))
                        UncondJumpInstruction(basicBlockIndex, newBasicBlock.ID.Value);
                }
                embeddedBasicBlocks.Clear();

                //Adding a fake unconditional jump that will be used later during unconditional meshing
                if (trueBasicBlock.Instruction.Last.StatementType.EnumerationValue != StatementTypeType.EnumValues.eConditionalJump)
                    UncondJumpInstruction(lastTrueBasicBlockIndex, newBasicBlock.ID.Value);

                //If we are in an inner scope, then we add this basic block as an embedded one and process the remaining instructions
                if (innerScope)
                    lineIndex = ProcessInnerScope(lineIndex - 1);
            }
            //If we are at the end of another scope, we add both last true and last false basic blocks to the embedded list
            else
            {
                embeddedBasicBlocks.Add(lastTrueBasicBlockIndex);
                if (lastFalseBasicBlockIndex != -1)
                    embeddedBasicBlocks.Add(lastFalseBasicBlockIndex);
            }
            return lineIndex;
        }
        /// <summary>
        /// Creates a "for" instruction
        /// </summary>
        /// <param name="lineIndex">Index of the PC line in the array "original" </param>
        /// <returns>The correct line index after processing the inner scope of the instruction</returns>
        private static int ForInstruction(int lineIndex, bool innerScope)
        {
            //The parameter lineIndex indicates which line we are in in the original code
            int functionIndex = routine.Function.Count - 1;
            int predBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            bool logicalOperations = false;
            //Stores the index of the basic blocks that need linking after logical operations and which branch they belong to
            Dictionary<int, bool> logicalBasicBlocks = new Dictionary<int, bool>();

            //Extracting the control elements ( i = 2 i < 10 i++ )
            string aux = original[lineIndex].Substring(original[lineIndex].IndexOf('(') + 2);
            aux = aux.Remove(aux.Length - 2);
            string[] tokens = aux.Split(';');
            string[] tokens2;
            //Creating the copy instruction
            tokens[0] = tokens[0].Trim();
            if ((tokens[0].Contains("*") || tokens[0].Contains("-") || tokens[0].Contains("+") || tokens[0].Contains("/") || tokens[0].Contains("%")))
            {
                tokens2 = tokens[0].Split(' ');
                CopyInstruction(string.Join(" ", tokens2[0], tokens2[1], ProcessArithmeticOperations(tokens[0].Substring(tokens[0].IndexOf('=') + 1))));
            }
            else
                CopyInstruction(tokens[0]);

            //New Basic Block for the Conditional Jump
            BasicBlockType condJumpBasicBlock = routine.Function[functionIndex].BasicBlock.Append();
            condJumpBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            int condJumpBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            LinkPredecessor(functionIndex, condJumpBasicBlockIndex, predBasicBlockIndex);
            //Creating the conditional jump
            tokens[1] = tokens[1].Trim();
            //Checking whether we have logical operations in the condition
            if (tokens[1].Contains("||") || tokens[1].Contains("&&"))
            {
                logicalBasicBlocks = ProcessLogicalOperations(tokens[1]);
                logicalOperations = true;
            }
            else
                PreCondJump(tokens[1]);

            //New Basic Block for the Inner Scope
            BasicBlockType innerScopeBasicBlock = routine.Function[functionIndex].BasicBlock.Append();
            innerScopeBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            int firstInnerScopeBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            //Only if we do not have logical operations. If we do have, the liking will be applied later
            if (!logicalOperations)
            {
                LinkPredecessor(functionIndex, firstInnerScopeBasicBlockIndex, condJumpBasicBlockIndex);
                CompleteCondJump(functionIndex, condJumpBasicBlockIndex, innerScopeBasicBlock.ID.Value);
            }
            //tokens[2] is something like "i++"
            tokens[2] = tokens[2].Trim();
            //Processing instructions between { } 
            lineIndex = ProcessInnerScope(lineIndex, tokens[2]);
            int lastInnerScopeBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            LinkPredecessor(functionIndex, lastInnerScopeBasicBlockIndex, lastInnerScopeBasicBlockIndex - 1);
            LinkPredecessor(functionIndex, condJumpBasicBlockIndex, lastInnerScopeBasicBlockIndex);
            UncondJumpInstruction(lastInnerScopeBasicBlockIndex, routine.Function[functionIndex].BasicBlock[condJumpBasicBlockIndex].ID.Value);

            //Linking embedded basic blocks (if they exist)
            LinkEmbeddedBasicBlocks(functionIndex, lastInnerScopeBasicBlockIndex);

            //New Basic Block for the intructions after the For Loop
            BasicBlockType newBasicBlock = routine.Function[functionIndex].BasicBlock.Append();
            newBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            int newBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            //Linking the basic blocks created by logical operations (if they exist)
            if (logicalOperations)
                LinkLogicalBasicBlocks(logicalBasicBlocks, firstInnerScopeBasicBlockIndex, newBasicBlockIndex);
            else
                LinkPredecessor(functionIndex, newBasicBlockIndex, condJumpBasicBlockIndex);

            return lineIndex;
        }
        /// <summary>
        /// Creates a "DoWhile" instruction
        /// </summary>
        /// <param name="lineIndex">Index of the PC line in the array "original" </param>
        /// <returns>The correct line index after processing the inner scope of the instruction</returns>
        private static int DoWhileInstruction(int lineIndex, bool innerScope)
        {
            //The parameter lineIndex indicates which line we are in in the original code
            int functionIndex = routine.Function.Count - 1;
            int predBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            bool logicalOperations = false;
            //Stores the index of the basic blocks that need linking after logical operations and which branch they belong to
            Dictionary<int, bool> logicalBasicBlocks = new Dictionary<int, bool>();

            //New Basic Block for the Inner Scope
            BasicBlockType innerScopeBasicBlock = routine.Function[functionIndex].BasicBlock.Append();
            innerScopeBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            int firstInnerScopeBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            LinkPredecessor(functionIndex, firstInnerScopeBasicBlockIndex, predBasicBlockIndex);
            //Processing instructions between { }
            lineIndex = ProcessInnerScope(lineIndex);
            int lastInnerScopeBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;

            //New Basic Block for the Conditional Jump
            BasicBlockType condJumpBasicBlock = routine.Function[functionIndex].BasicBlock.Append();
            condJumpBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            int condJumpBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            LinkPredecessor(functionIndex, condJumpBasicBlockIndex, lastInnerScopeBasicBlockIndex);
            //Extracting the control elements
            string aux = original[lineIndex].Substring(original[lineIndex].IndexOf('(') + 2);
            aux = aux.Remove(aux.Length - 2);
            //Checking whether we have logical operations in the condition
            if (aux.Contains("||") || aux.Contains("&&"))
            {
                logicalBasicBlocks = ProcessLogicalOperations(aux);
                logicalOperations = true;
            }
            else
            {
                PreCondJump(aux);
                LinkPredecessor(functionIndex, firstInnerScopeBasicBlockIndex, condJumpBasicBlockIndex);
                CompleteCondJump(functionIndex, condJumpBasicBlockIndex, innerScopeBasicBlock.ID.Value);
            }

            //Linking embedded basic blocks (if they exist)
            LinkEmbeddedBasicBlocks(functionIndex, lastInnerScopeBasicBlockIndex);

            //New Basic Block for the intructions after the Do While Loop
            BasicBlockType newBasicBlock = routine.Function[functionIndex].BasicBlock.Append();
            newBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            int newBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            //Linking the basic blocks created by logical operations (if they exist)
            if (logicalOperations)
                LinkLogicalBasicBlocks(logicalBasicBlocks, firstInnerScopeBasicBlockIndex, newBasicBlockIndex);
            else
                LinkPredecessor(functionIndex, newBasicBlockIndex, condJumpBasicBlockIndex);

            return lineIndex + 1;
        }
        /// <summary>
        /// Creates a "while" instruction
        /// </summary>
        /// <param name="lineIndex">Index of the PC line in the array "original" </param>
        /// <returns>The correct line index after processing the inner scope of the instruction</returns>
        private static int WhileInstruction(int lineIndex, bool innerScope)
        {
            int functionIndex = routine.Function.Count - 1;
            int predBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            bool logicalOperations = false;
            //Stores the index of the basic blocks that need linking after logical operations and which branch they belong to
            Dictionary<int, bool> logicalBasicBlocks = new Dictionary<int, bool>();

            //New Basic Block for the Conditional Jump
            BasicBlockType condJumpBasicBlock = routine.Function[functionIndex].BasicBlock.Append();
            condJumpBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            int condJumpBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            LinkPredecessor(functionIndex, condJumpBasicBlockIndex, predBasicBlockIndex);
            //Extracting the control elements
            string aux = original[lineIndex].Substring(original[lineIndex].IndexOf('(') + 2);
            aux = aux.Remove(aux.Length - 2);
            //Checking whether we have logical operations in the condition
            if (aux.Contains("||") || aux.Contains("&&"))
            {
                logicalBasicBlocks = ProcessLogicalOperations(aux);
                logicalOperations = true;
            }
            else
                PreCondJump(aux);

            //New Basic Block for the Inner Scope
            BasicBlockType innerScopeBasicBlock = routine.Function[functionIndex].BasicBlock.Append();
            innerScopeBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            int firstInnerScopeBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            //Only if we do not have logical operations. If we do have, the liking will be applied later
            if (!logicalOperations)
            {
                LinkPredecessor(functionIndex, firstInnerScopeBasicBlockIndex, condJumpBasicBlockIndex);
                CompleteCondJump(functionIndex, condJumpBasicBlockIndex, innerScopeBasicBlock.ID.Value);
            }
            //Processing instructions between { }
            lineIndex = ProcessInnerScope(lineIndex);
            int lastInnerScopeBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            UncondJumpInstruction(lastInnerScopeBasicBlockIndex, routine.Function[functionIndex].BasicBlock[condJumpBasicBlockIndex].ID.Value);
            LinkPredecessor(functionIndex, condJumpBasicBlockIndex, lastInnerScopeBasicBlockIndex);

            //Linking embedded basic blocks (if they exist)
            LinkEmbeddedBasicBlocks(functionIndex, lastInnerScopeBasicBlockIndex);

            //New Basic Block for the intructions after the While Loop
            BasicBlockType newBasicBlock = routine.Function[functionIndex].BasicBlock.Append();
            newBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            int newBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            //Linking the basic blocks created by logical operations (if they exist)
            if (logicalOperations)
                LinkLogicalBasicBlocks(logicalBasicBlocks, firstInnerScopeBasicBlockIndex, newBasicBlockIndex);
            else
                LinkPredecessor(functionIndex, newBasicBlockIndex, condJumpBasicBlockIndex);

            return lineIndex;
        }
        /// <summary>
        /// Preprocesses the line before creating the conditional jump instruction
        /// </summary>
        /// <param name="line">PC line, similar to "i" or "i < 100" or "i * a1 > 100"</param>
        private static void PreCondJump(string line)
        {
            string[] tokens = line.Split(' ');
            //line is something like i (true)
            if (tokens.Length == 1)
                CondJumpInstruction(string.Concat(tokens[0], " > 0"));
            //line is something similar to "i < 100"
            else if (tokens.Length == 3)
                CondJumpInstruction(line);
            //line is something similar to "i * a1 < 100 "
            else
                CondJumpInstruction(string.Join(" ", ProcessArithmeticOperations(line), tokens[tokens.Length - 2], tokens[tokens.Length - 1]));
        }
        /// <summary>
        /// Creates a conditional jump instruction
        /// </summary>
        /// <param name="line">PC line, similar to "i < a1"</param>
        private static void CondJumpInstruction(string line)
        {
            string[] tokens = line.Split(' ');
            int functionIndex = routine.Function.Count - 1;
            int basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            InstructionType newInstruction = routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction.Append();
            newInstruction.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newInstruction.StatementType.EnumerationValue = StatementTypeType.EnumValues.eConditionalJump;

            //Dealing with pointers
            string[] forPointers = { string.Empty, string.Empty };
            tokens[0] = DealWithPointers(tokens[0], out forPointers[0]);
            tokens[2] = DealWithPointers(tokens[2], out forPointers[1]);

            string refVars = string.Empty;
            //Adding i
            if (!GetIDVariable(tokens[0]).Equals(tokens[0]))
                refVars = string.Concat(refVars, GetIDVariable(tokens[0]));
            //Adding a1
            if (!GetIDVariable(tokens[2]).Equals(tokens[2]))
                if (refVars.Length != 0)
                    refVars = string.Concat(refVars, string.Concat(" ", GetIDVariable(tokens[2])));
                else
                    refVars = string.Concat(refVars, GetIDVariable(tokens[2]));
            newInstruction.RefVars.Value = refVars;
            newInstruction.Value = string.Join("", "if ", forPointers[0], GetValueVariable(tokens[0]), " ", tokens[1], " ",
                forPointers[1], GetValueVariable(tokens[2]), " goto ");
        }
        /// <summary>
        /// Creates a unconditional jump instruction
        /// </summary>
        /// <param name="indexOwnerBasicBlock">Index of the basic block holding the unconditional jump</param>
        /// <param name="targetBasicBlockID">Target basic block ID</param>
        private static void UncondJumpInstruction(int indexOwnerBasicBlock, string targetBasicBlockID)
        {
            //The parameter basicBlockID is expected to have a content similar to "ID_80D994CA-833A-4F05-99A9-EC3BD2DEA9EE"
            int functionIndex = routine.Function.Count - 1;
            InstructionType newInstruction = routine.Function[functionIndex].BasicBlock[indexOwnerBasicBlock].Instruction.Append();
            newInstruction.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newInstruction.StatementType.EnumerationValue = StatementTypeType.EnumValues.eUnconditionalJump;
            newInstruction.Value = string.Concat("goto ", targetBasicBlockID);
        }
        /// <summary>
        /// Processes the inner scope of a instruction
        /// </summary>
        /// <param name="lineIndex">Index of the PC line in the array "original" </param>
        /// <param name="operation">Operation to be insert at the end of the inner scope</param>
        /// <returns>The correct line index after processing the inner scope of the instruction</returns>
        private static int ProcessInnerScope(int lineIndex, string operation = "")
        {
            int auxIndex = lineIndex + 1;
            if (original[auxIndex].Equals("{"))
            {
                auxIndex++;
                //while it is not the end of the inner scope
                while (!original[auxIndex].Equals("}"))
                {
                    auxIndex = CreateInstruction(auxIndex, true);
                }
                auxIndex++;
            }
            else
                auxIndex = CreateInstruction(auxIndex, true);


            //In case we are processing an inner scope of a for loop. This operation would be
            //something like i++
            if (operation.Length > 0)
            {
                int functionIndex = routine.Function.Count - 1;
                int previousBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
                //A new basic block is created to avoid problems with embedded "IFs"
                BasicBlockType operationBasicBlock = routine.Function[functionIndex].BasicBlock.Append();
                operationBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
                PreFullAssignCopy(0, operation);
            }
            return auxIndex;
        }
        /// <summary>
        /// Processes a sequence of arithmetic operations
        /// </summary>
        /// <param name="line">Line similar to "a1 + v5" or "a1 + v5 + a2...""</param>
        /// <returns>The temporary variable that will hold the sequence operations' result</returns>
        private static string ProcessArithmeticOperations(string line)
        {
            if (line.Contains("/") || line.Contains("%"))
                functionsHasDivisionModulo = true;
            line = line.Trim();
            string tempVariable1 = string.Empty;
            string tempVariable2;
            string[] tokens = line.Split(' ');
            int i = 0;
            while (i + 1 < tokens.Length && (tokens[i + 1].Equals("*") || tokens[i + 1].Equals("-") || tokens[i + 1].Equals("+")
                || tokens[i + 1].Equals("/") || tokens[i + 1].Equals("%")))
            {
                if (tempVariable1.Length == 0)
                {
                    tempVariable1 = string.Concat("t_", randNumbers.Next().ToString());
                    CreateLocalVariable(string.Concat("int ", tempVariable1));
                    FullAssignInstruction(string.Join(" ", tempVariable1, "=", tokens[i], tokens[i + 1], tokens[i + 2]));
                }
                else
                {
                    tempVariable2 = string.Concat("t_", randNumbers.Next().ToString());
                    CreateLocalVariable(string.Concat("int ", tempVariable2));
                    FullAssignInstruction(string.Join(" ", tempVariable2, "=", tempVariable1, tokens[i + 1], tokens[i + 2]));
                    tempVariable1 = tempVariable2;
                }
                i += 2;
            }
            return tempVariable1;
        }
        /// <summary>
        /// Processes logical operations
        /// </summary>
        /// <param name="line">Line similar to "a1 < 100 || a1 >= 200"</param>
        /// <returns>The last condJumpBasicBlock index</returns>
        private static Dictionary<int, bool> ProcessLogicalOperations(string line)
        {
            int functionIndex = routine.Function.Count - 1;
            int predBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            int condJumpBasicBlockIndex;
            int uncondJumpBasicBlockIndex;
            int nextCondJumpBasicBlockIndex;
            Dictionary<int, bool> logicalBasicBlocks = new Dictionary<int, bool>();

            line = line.Replace("||", "|@");
            line = line.Replace("&&", "&@");
            string[] tokens = line.Split('@');
            PreCondJump(tokens[0].Substring(0, tokens[0].Length - 2));
            if (tokens[0].Contains("|"))
                logicalBasicBlocks.Add(predBasicBlockIndex, true);
            else
                logicalBasicBlocks.Add(predBasicBlockIndex, false);
            //New basic block for the next conditional jump
            BasicBlockType condJumpBasicBlock = routine.Function[functionIndex].BasicBlock.Append();
            condJumpBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            condJumpBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            if (tokens[0].Contains("&"))
            {
                LinkPredecessor(functionIndex, condJumpBasicBlockIndex, predBasicBlockIndex, true);
                CompleteCondJump(functionIndex, predBasicBlockIndex, condJumpBasicBlock.ID.Value);
            }
            else
                LinkPredecessor(functionIndex, condJumpBasicBlockIndex, predBasicBlockIndex);
            for (int i = 1; i < tokens.Length; i++)
            {
                tokens[i] = tokens[i].TrimStart();

                if (tokens[i].Contains("|") || tokens[i].Contains("&"))
                    PreCondJump(tokens[i].Substring(0, tokens[i].Length - 2));
                else
                    PreCondJump(tokens[i]);

                if (tokens[i].Contains("|") && !tokens[i - 1].Contains("&"))
                    logicalBasicBlocks.Add(condJumpBasicBlockIndex, true);
                else
                {
                    BasicBlockType uncondJumpBasicBlock = routine.Function[functionIndex].BasicBlock.Append();
                    uncondJumpBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
                    uncondJumpBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
                    logicalBasicBlocks.Add(uncondJumpBasicBlockIndex, false);
                    LinkPredecessor(functionIndex, uncondJumpBasicBlockIndex, condJumpBasicBlockIndex);
                }

                if (i < tokens.Length - 1)
                {
                    BasicBlockType nextCondJumpBasicBlock = routine.Function[functionIndex].BasicBlock.Append();
                    nextCondJumpBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
                    nextCondJumpBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
                    if (tokens[i].Contains("&"))
                    {
                        CompleteCondJump(functionIndex, condJumpBasicBlockIndex, nextCondJumpBasicBlock.ID.Value);
                        LinkPredecessor(functionIndex, nextCondJumpBasicBlockIndex, condJumpBasicBlockIndex, true);
                    }
                    else if (tokens[i - 1].Contains("&") && tokens[i].Contains("|"))
                    {
                        logicalBasicBlocks.Add(condJumpBasicBlockIndex, true);
                        LinkLogicalBasicBlocks(logicalBasicBlocks, -1, nextCondJumpBasicBlockIndex, true);
                    }
                    else
                        LinkPredecessor(functionIndex, nextCondJumpBasicBlockIndex, condJumpBasicBlockIndex);
                    condJumpBasicBlockIndex = nextCondJumpBasicBlockIndex;
                }
                //Adding the last conditional jump
                else
                    logicalBasicBlocks.Add(condJumpBasicBlockIndex, true);
            }
            return logicalBasicBlocks;
        }
        /// <summary>
        /// Links basic blocks created  due to logical operations
        /// </summary>
        /// <param name="trueBasicBlockIndex">Basic block to be linked to in the true branch</param>
        /// <param name="falseBasicBlockIndex">Basic block to be linked to in the false branch</param>
        /// <param name="partial">Flag to indicate that partial linking should be performed (only false branch)</param>
        private static void LinkLogicalBasicBlocks(Dictionary<int, bool> logicalBasicBlocks, int trueBasicBlockIndex, int falseBasicBlockIndex, bool partial = false)
        {
            int functionIndex = routine.Function.Count - 1;
            List<int> linkedPartial = new List<int>();
            BasicBlockType trueBasicBlock = routine.Function[functionIndex].BasicBlock[trueBasicBlockIndex];
            BasicBlockType falseBasicBlock = routine.Function[functionIndex].BasicBlock[falseBasicBlockIndex];
            foreach (int basicBlockIndex in logicalBasicBlocks.Keys)
            {
                BasicBlockType basicBlock = routine.Function[functionIndex].BasicBlock[basicBlockIndex];

                if (!partial && logicalBasicBlocks[basicBlockIndex] == true)
                {
                    if (basicBlock.Instruction.Exists)
                    {
                        LinkPredecessor(functionIndex, trueBasicBlockIndex, basicBlockIndex, true);
                        CompleteCondJump(functionIndex, basicBlockIndex, trueBasicBlock.ID.Value);
                    }
                    else
                    {
                        LinkPredecessor(functionIndex, trueBasicBlockIndex, basicBlockIndex);
                        UncondJumpInstruction(basicBlockIndex, trueBasicBlock.ID.Value);
                    }
                }
                else if (logicalBasicBlocks[basicBlockIndex] == false)
                {
                    if (basicBlock.Instruction.Exists)
                    {
                        LinkPredecessor(functionIndex, falseBasicBlockIndex, basicBlockIndex);
                        linkedPartial.Add(basicBlockIndex);
                    }
                    else
                    {
                        LinkPredecessor(functionIndex, falseBasicBlockIndex, basicBlockIndex);
                        UncondJumpInstruction(basicBlockIndex, falseBasicBlock.ID.Value);
                        linkedPartial.Add(basicBlockIndex);
                    }
                }
            }
            foreach (int basicBlockIndex in linkedPartial)
                logicalBasicBlocks.Remove(basicBlockIndex);
        }
        /// <summary>
        /// Links two basic blocks
        /// </summary>
        /// <param name="funcIndex">Index of the function which the basic blocks belong to</param>
        /// <param name="actualIndex">Actual basic block index</param>
        /// <param name="predIndex">Predecessor basic block index</param>
        private static void LinkPredecessor(int funcIndex, int actualIndex, int predIndex, bool first = false)
        {

            if (!routine.Function[funcIndex].BasicBlock[actualIndex].Predecessors.Exists())
                routine.Function[funcIndex].BasicBlock[actualIndex].Predecessors.Value =
                    routine.Function[funcIndex].BasicBlock[predIndex].ID.Value;
            else
            {
                if (!routine.Function[funcIndex].BasicBlock[actualIndex].Predecessors.Value.Contains(
                    routine.Function[funcIndex].BasicBlock[predIndex].ID.Value))
                {
                    routine.Function[funcIndex].BasicBlock[actualIndex].Predecessors.Value =
                        string.Join(" ", routine.Function[funcIndex].BasicBlock[actualIndex].Predecessors.Value,
                        routine.Function[funcIndex].BasicBlock[predIndex].ID.Value);
                }
            }

            if (!routine.Function[funcIndex].BasicBlock[predIndex].Successors.Exists())
                routine.Function[funcIndex].BasicBlock[predIndex].Successors.Value =
                    routine.Function[funcIndex].BasicBlock[actualIndex].ID.Value;
            else
            {
                if (!routine.Function[funcIndex].BasicBlock[predIndex].Successors.Value.Contains(
                    routine.Function[funcIndex].BasicBlock[actualIndex].ID.Value))
                {
                    if (first)
                        routine.Function[funcIndex].BasicBlock[predIndex].Successors.Value =
                        string.Join(" ", routine.Function[funcIndex].BasicBlock[actualIndex].ID.Value,
                        routine.Function[funcIndex].BasicBlock[predIndex].Successors.Value);
                    else
                        routine.Function[funcIndex].BasicBlock[predIndex].Successors.Value =
                        string.Join(" ", routine.Function[funcIndex].BasicBlock[predIndex].Successors.Value,
                            routine.Function[funcIndex].BasicBlock[actualIndex].ID.Value);
                }
            }
        }
        /// <summary>
        /// Links basic blocks embedded in loop bodies
        /// </summary>
        /// <param name="funcIndex">Index of the function which the basic blocks belong to</param>
        /// <param name="targetBasicBlockIndex">Target basic block index</param>
        private static void LinkEmbeddedBasicBlocks(int funcIndex, int targetBasicBlockIndex)
        {
            BasicBlockType targetBasicBlock = routine.Function[funcIndex].BasicBlock[targetBasicBlockIndex];
            foreach (int basicBlockIndex in embeddedBasicBlocks)
            {
                LinkPredecessor(funcIndex, targetBasicBlockIndex, basicBlockIndex);
                //If we don't have an unconditional jump to new basic block, we have to create it
                if (!routine.Function[funcIndex].BasicBlock[basicBlockIndex].Instruction.Exists ||
                    (routine.Function[funcIndex].BasicBlock[basicBlockIndex].Instruction.Exists &&
                    routine.Function[funcIndex].BasicBlock[basicBlockIndex].Instruction.Last.StatementType.EnumerationValue !=
                    StatementTypeType.EnumValues.eUnconditionalJump))
                    UncondJumpInstruction(basicBlockIndex, targetBasicBlock.ID.Value);
            }
            embeddedBasicBlocks.Clear();
        }
        /// <summary>
        /// Completes a conditional jump instruction with its target
        /// </summary>
        /// <param name="funcIndex">Index of the function which the basic block belongs to</param>
        /// <param name="indexOwnerBasicBlock">Index of the basic block holding the conditional jump</param>
        /// <param name="targetBasicBlockID">Conditional jump target ID</param>
        private static void CompleteCondJump(int funcIndex, int indexOwnerBasicBlock, string targetBasicBlockID)
        {
            int instructionIndex = routine.Function[funcIndex].BasicBlock[indexOwnerBasicBlock].Instruction.Count - 1;
            if (!routine.Function[funcIndex].BasicBlock[indexOwnerBasicBlock].Instruction[instructionIndex].Value.Contains(targetBasicBlockID))
                routine.Function[funcIndex].BasicBlock[indexOwnerBasicBlock].Instruction[instructionIndex].Value = string.Concat(
                    routine.Function[funcIndex].BasicBlock[indexOwnerBasicBlock].Instruction[instructionIndex].Value, targetBasicBlockID);
        }
        /// <summary>
        /// Gets the ID of a variable
        /// </summary>
        /// <param name="globalID">Variable's global ID</param>
        /// <returns>Either the ID of the variable if it exists in the routine or the globalID if it does not</returns>
        private static string GetIDVariable(string globalID)
        {
            for (int i = 0; i < currentFunctionLocalVars.Variable.Count; i++)
            {
                if (currentFunctionLocalVars.Variable[i].GlobalID.Value == globalID)
                    return currentFunctionLocalVars.Variable[i].ID.Value;
            }
            return globalID;
        }
        /// <summary>
        /// Gets the value of a variable
        /// </summary>
        /// <param name="globalID">Variable's global ID</param>
        /// <returns>Either the value of the variable if it exists or the globalID if it does not</returns>
        private static string GetValueVariable(string globalID)
        {
            for (int i = 0; i < currentFunctionLocalVars.Variable.Count; i++)
            {
                if (currentFunctionLocalVars.Variable[i].GlobalID.Value == globalID)
                    return currentFunctionLocalVars.Variable[i].Value;
            }
            return globalID;
        }
        /// <summary>
        /// Gets the memory region size of a variable
        /// </summary>
        /// <param name="globalID">Variable's global ID</param>
        /// <returns>The memory region size</returns>
        private static int GetMemoryRegionSizeVariable(string globalID)
        {
            int i = 0;
            while (i < currentFunctionLocalVars.Variable.Count && currentFunctionLocalVars.Variable[i].GlobalID.Value != globalID)
                i++;
            return (int)currentFunctionLocalVars.Variable[i].MemoryRegionSize.Value;
        }
        /// <summary>
        /// Gets the ID of a function
        /// </summary>
        /// <param name="globalID">Function's global ID</param>
        /// <returns>Either the ID of the function if it exists in the routine or the globalID if it does not</returns>
        private static string GetIDFunction(string globalID)
        {

            for (int i = 0; i < routine.Function.Count; i++)
            {
                if (routine.Function[i].GlobalID.Value == globalID)
                    return routine.Function[i].ID.Value;
            }
            return globalID;
        }
        /// <summary>
        /// Gets the index of a basic block
        /// </summary>
        /// <param name="funcIndex">Index of the function which the basic block belongs to</param>
        /// <param name="basicblockID">Basic block's ID</param>
        /// <returns>The given basic block's ID</returns>
        private static int GetIndexBasicBlock(int funcIndex, string basicblockID)
        {
            int index = 0;
            foreach (BasicBlockType bb in routine.Function[funcIndex].BasicBlock)
            {
                if (bb.ID.Equals(basicblockID))
                    break;
                index++;
            }
            return index;
        }
        /// <summary>
        /// Checks whether we are working with pointers and apply the necessary operations
        /// </summary>
        /// <param name="globalID">Global ID of the possible pointer</param>
        /// <param name="difOperator">Output parameter that have the deference operator</param>
        /// <returns>The gloabl ID that should be used</returns>
        private static string DealWithPointers(string globalID, out string defOperator)
        {
            defOperator = string.Empty;
            if (globalID.IndexOf('*') != -1)
            {
                defOperator = "* ";
                return globalID.Remove(globalID.IndexOf('*'), 1);
            }
            else if (globalID.IndexOf('&') != -1)
            {
                defOperator = "& ";
                return globalID.Remove(globalID.IndexOf('&'), 1);
            }
            return globalID;
        }
    }
}

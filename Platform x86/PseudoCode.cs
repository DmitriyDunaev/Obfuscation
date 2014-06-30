using ExchangeFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Platform_x86
{
    public static class PseudoCode
    {
        private static RoutineType routine;
        private static Variables currentFunctionLocalVars;
        private static string currentFunctionReturnType;
        private static string[] original; //Stores the original pseudocode
        private static Random randNumbers = new Random(DateTime.Now.Millisecond);
        
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
        private static void PreProc()
        {
            string[] toReplacePC = { "_cdecl ", "*(_BYTE *)", "*(_DWORD *)", ",", ";", "signed ", "unsigned ", "(signed ", "(unsigned " }; //Array of unnecessary elements
            string[] toReplaceTAC = { "", "", "", "", "", "", "", "(", "(" };
            for (int i = 0; i < original.Length; i++)
            {
                //Replacing unnecessary elements
                for (int j = 0; j < toReplacePC.Length; j++) 
                {
                    if (original[i].Contains(toReplacePC[j]))
                        original[i] = original[i].Replace(toReplacePC[j], toReplaceTAC[j]);
                }
                //Removing comments
                if (original[i].Contains("//")) 
                    original[i] = original[i].Remove(original[i].IndexOf("//"));
                original[i] = original[i].TrimStart();
                original[i] = original[i].TrimEnd();
            }
        }
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
                            if (original[i].Contains("_sub")) 
                            {
                                CreateFunction(original[i]);
                                translated = true;
                            }
                            //For variables
                            else 
                            {
                                CreateLocalVariable(original[i]);
                                translated = true;
                            }
                        }
                    }
                    //For instructions
                    if (translated == false)
                        //The index will be updated accordingly to the number of lines consumed by the instruction
                        i = CreateInstruction(i); 
                }
                i++;
            }
        }
        private static void CreateFunction(string line)
        {
            //The parameter line is expected to have a content similar to "int sub_401334(int a1 int a2)"
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
            if (!line.Contains("()"))
            {
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
                        inputVars = string.Join(" ",inputVars, GetIDVariable(tokens[i])); 
                    else
                        inputVars = GetIDVariable(tokens[i]);                    
                }
                routine.Function[routine.Function.Count - 1].RefInputVars.Value = inputVars;
            }
            BasicBlockType newBasicBlock = newFunction.BasicBlock.Append();
            newBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newBasicBlock.Successors.Value = string.Empty;
        }
        private static void CreateLocalVariable(string line)
        {
            //The parameter line is expected to have a content similar to "int a int b int c"
            bool pointer;
            string[] tokens = line.Split(' ');
            VariableType newVariable;
            for (int i = 0; i < tokens.Length - 1; i += 2)
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
                newVariable.ID.Value = string.Concat("ID_",Guid.NewGuid().ToString().ToUpper());
                newVariable.Value = string.Concat("v_",newVariable.ID.Value);
                newVariable.GlobalID.Value = tokens[i + 1];
                newVariable.Pointer.Value = pointer;
                if (tokens[i].Equals("char"))
                    newVariable.MemoryRegionSize.Value = 1;               
                else
                    newVariable.MemoryRegionSize.Value = 4;               
            }
        }
        private static int CreateInstruction(int lineIndex)
        {
            if (original[lineIndex].Contains("if"))
            {
                lineIndex = IfInstruction(lineIndex);
            }
            else if (original[lineIndex].Contains("for"))
            {
                lineIndex = ForInstruction(lineIndex);
            }
            else if (original[lineIndex].Contains("do"))
            {
                lineIndex = DoWhileInstruction(lineIndex);
            }
            else if (original[lineIndex].Contains("while"))
            {
                lineIndex = WhileInstruction(lineIndex);
            }
            else if (original[lineIndex].Contains("return"))
            {
                PreReturn(lineIndex);
            }
            else if (original[lineIndex].IndexOf("sub_") != -1)
            {
                CallInstruction(original[lineIndex]);
            }
            else if (original[lineIndex].IndexOf('=') != -1 || original[lineIndex].IndexOf("++") != -1 || original[lineIndex].IndexOf("--") != -1)
            {
                PreFullAssignCopy(lineIndex);
            }
            else
                Console.Write("Instruction not implemented: " + original[lineIndex]);
            return lineIndex;
        }
        private static void PreReturn(int lineIndex)
        {
            string aux = original[lineIndex];
            //aux is something like "return sub_401334(a)"
            if (aux.Contains("sub_")) 
            {
                aux = aux.Substring(aux.IndexOf("sub_")); //sub_401334(a)
                aux = aux.Remove(aux.IndexOf('(')); //sub_401334
                CreateLocalVariable(string.Join(" ", currentFunctionReturnType, aux));
                CallInstruction(original[lineIndex].Substring(original[lineIndex].IndexOf("sub_")));
                RetrieveInstruction(original[lineIndex].Substring(original[lineIndex].IndexOf("sub_")));
                ReturnInstruction(string.Concat("return ",aux));
            }
            //aux is something like "return a1 + v5"
            else if (aux.Contains("+") || aux.Contains("-") || aux.Contains("*") || aux.Contains("/")) 
            {
                string tempVariable1 = string.Empty;
                string tempVariable2;
                string[] tokens = aux.Split(' ');
                int i = 1;
                while (i + 1 < tokens.Length && (tokens[i + 1].Equals("*") || tokens[i + 1].Equals("-") || tokens[i + 1].Equals("+") || tokens[i + 1].Equals("/")))
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
                ReturnInstruction(string.Concat("return ", tempVariable1));
            }
            //aux is something like "return 0" or "return a1"
            else 
            {
                ReturnInstruction(aux);
            }
        }
        private static void ReturnInstruction(string line)
        {
            //The parameter line is expected to have a content similar to "return v3"
            string[] tokens = line.Split(' ');
            int indexFunction = routine.Function.Count - 1;
            int indexBasicblock = routine.Function[indexFunction].BasicBlock.Count - 1;
            InstructionType newInstruction = routine.Function[indexFunction].BasicBlock[indexBasicblock].Instruction.Append();
            newInstruction.ID.Value = string.Concat("ID_",Guid.NewGuid().ToString().ToUpper());
            newInstruction.StatementType.EnumerationValue = StatementTypeType.EnumValues.eProcedural;
            newInstruction.PolyRequired.Value = false;
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
        private static void CallInstruction(string line)
        {
            //The parameter line is expected to have a content similar to "sub_401334(a, b)"
            int numParams = 0;
            int indexFunction = routine.Function.Count - 1;
            int indexBasicblock = routine.Function[indexFunction].BasicBlock.Count - 1;
            InstructionType newInstruction;
            //Dealing with parameters
            if (!line.Contains("()")) 
            {
                //Extracting parameters
                string aux = line.Substring(line.IndexOf('(') + 1); 
                aux = aux.Replace(")", "");
                string[] tokens = aux.Split(' ');
                //Creating "param" instructions
                foreach (string token in tokens)
                {
                    newInstruction = routine.Function[indexFunction].BasicBlock[indexBasicblock].Instruction.Append();
                    newInstruction.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
                    newInstruction.StatementType.EnumerationValue = StatementTypeType.EnumValues.eProcedural;
                    newInstruction.PolyRequired.Value = false;
                    newInstruction.RefVars.Value = GetIDVariable(token);
                    newInstruction.Value = string.Concat("param ", GetValueVariable(token));
                    numParams++;
                }
            }
            //Creating the "call" instruction
            string[] tokens2 = line.Split('(');
            newInstruction = routine.Function[indexFunction].BasicBlock[indexBasicblock].Instruction.Append();
            newInstruction.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newInstruction.StatementType.EnumerationValue = StatementTypeType.EnumValues.eProcedural;
            newInstruction.PolyRequired.Value = false;
            newInstruction.Value = string.Join(" ", "call", GetIDFunction(tokens2[0]), numParams);
            for (int i = 0; i < routine.Function.Count; i++)
            {
                if (routine.Function[i].GlobalID.Value == tokens2[0])
                    routine.Function[i].CalledFrom.EnumerationValue = CalledFromType.EnumValues.eBoth;
            }
        }
        private static void RetrieveInstruction(string line)
        {
            //The parameter line is expected to have a content similar to "sub_401334(a, b)"
            string[] tokens = line.Split('(');
            int indexFunction = routine.Function.Count - 1;
            int indexBasicblock = routine.Function[indexFunction].BasicBlock.Count - 1;
            InstructionType newInstruction = newInstruction = routine.Function[indexFunction].BasicBlock[indexBasicblock].Instruction.Append();
            newInstruction.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newInstruction.StatementType.EnumerationValue = StatementTypeType.EnumValues.eProcedural;
            newInstruction.PolyRequired.Value = false;
            newInstruction.RefVars.Value = GetIDVariable(tokens[0]);
            newInstruction.Value = string.Concat("retrieve ", GetValueVariable(tokens[0]));
        }
        private static void PreFullAssignCopy(int lineIndex, string operation = "")
        {
            string aux = operation;
            if (operation.Length == 0)
            {
                aux = original[lineIndex];
            }
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
            else if (tokens.Length == 3 && ((aux.Contains("+=") || aux.Contains("-=") || aux.Contains("*=") || aux.Contains("/=")))) //aux is something like "v5 += v1"
            {
                tokens[1] = tokens[1].Remove(tokens[1].IndexOf('='), 1);
                FullAssignInstruction(string.Join(" ", tokens[0], "=", tokens[2], tokens[1], tokens[0]));
            }
            //aux is something like "v5 = v1" or "v5 = v1 + v2" or "v5 = v1 += v2" or "v5 = v1 = v2"
            else 
            {
                //aux is "v5 = v1"
                if (tokens.Length == 3) 
                {
                    CopyInstruction(aux);
                }
                else
                {
                    
                    int i = tokens.Length - 1;
                    while (i > 1)
                    {
                        //aux is "v5 = v1 + v2"
                        if (tokens[i - 3].Equals("=") && ((tokens[i - 1].Equals("+") || tokens[i - 1].Equals("-") || tokens[i - 1].Equals("*") || tokens[i - 1].Equals("/")))) 
                        {
                            string tempVariable = string.Concat("t_", randNumbers.Next().ToString());
                            CreateLocalVariable(string.Concat("int ", tempVariable));
                            FullAssignInstruction(string.Join(" ", tempVariable, tokens[i - 3], tokens[i - 2], tokens[i - 1], tokens[i]));
                            CopyInstruction(string.Join(" ", tokens[i - 4], "=", tempVariable)); //v5 = v1
                            i -= 4;
                        }
                        //aux is "v5 = v1 += v2"
                        else if (tokens[i - 3].Equals("=") && ((tokens[i - 1].Equals("+=") || tokens[i - 1].Equals("-=")))) 
                        {
                            tokens[i - 1] = tokens[i - 1].Replace("=", "");
                            FullAssignInstruction(string.Join(" ", tokens[i - 2], "=", tokens[i], tokens[i - 1], tokens[i - 2])); //v1 = v2 + v1
                            CopyInstruction(string.Join(" ", tokens[i - 4], tokens[i - 3], tokens[i - 2])); //v5 = v1
                            i -= 4;
                        }
                        //aux is "v5 = v1 = v2"
                        else 
                        {
                            while (i > 0 && tokens[i - 1].Equals("="))
                            {
                                CopyInstruction(tokens[i - 2] + " " + tokens[i - 1] + " " + tokens[i]);
                                i -= 2;
                            }
                        }
                    }
                }
            }
        }
        private static void FullAssignInstruction(string line)
        {
            //The parameter line is expected to have a content similar to "v3 = a2 * a1"
            string[] tokens = line.Split(' ');
            int indexFunction = routine.Function.Count - 1;
            int indexBasicblock = routine.Function[indexFunction].BasicBlock.Count - 1;
            InstructionType newInstruction = routine.Function[indexFunction].BasicBlock[indexBasicblock].Instruction.Append();
            newInstruction.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newInstruction.StatementType.EnumerationValue = StatementTypeType.EnumValues.eFullAssignment;
            newInstruction.PolyRequired.Value = false;
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
            newInstruction.Value = string.Join(" ", GetValueVariable(tokens[0]), ":=",
                GetValueVariable(tokens[2]), tokens[3], GetValueVariable(tokens[4]));
        }
        private static void CopyInstruction(string input)
        {
            //The parameter "input" is expected to have a content similar to "v3 = 10" or "v3 = a1"
            string[] tokens = input.Split(' ');
            string[] forPointers = { "", "" };
            
            int functionIndex = routine.Function.Count - 1;
            int basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            InstructionType newInstruction = routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction.Append();
            newInstruction.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            if (input.IndexOf('*') != -1 || input.IndexOf('&') != -1)
                newInstruction.StatementType.EnumerationValue = StatementTypeType.EnumValues.ePointerAssignment;
            else
                newInstruction.StatementType.EnumerationValue = StatementTypeType.EnumValues.eCopy;
            newInstruction.PolyRequired.Value = false;
            //Dealing with pointers
            if (tokens[0].IndexOf('*') != -1) 
            {
                forPointers[0] = "*";
                tokens[0] = tokens[0].Remove(tokens[0].IndexOf('*'), 1);
            }
            else if (tokens[0].IndexOf('&') != -1)
            {
                forPointers[0] = "&";
                tokens[0] = tokens[0].Remove(tokens[0].IndexOf('&'), 1);
            }
            if (tokens[2].IndexOf('*') != -1)
            {
                forPointers[1] = "*";
                tokens[2] = tokens[2].Remove(tokens[2].IndexOf('*'), 1);
            }
            else if (tokens[2].IndexOf('&') != -1)
            {
                forPointers[1] = "&";
                tokens[2] = tokens[2].Remove(tokens[2].IndexOf('&'), 1);
            }
            string refVars = string.Empty;
            if (!GetIDVariable(tokens[0]).Equals(tokens[0]))
                    refVars = GetIDVariable(tokens[0]);
            if (!GetIDVariable(tokens[2]).Equals(tokens[2]))
                if (refVars.Length != 0)
                    refVars = string.Join(" ", refVars, GetIDVariable(tokens[2]));
                else
                    refVars = GetIDVariable(tokens[2]);                            
            newInstruction.RefVars.Value = refVars;
            newInstruction.Value = string.Concat(forPointers[0], string.Concat(GetValueVariable(tokens[0]), string.Concat(" := ", 
                string.Concat(forPointers[1], GetValueVariable(tokens[2])))));
        }
        private static int IfInstruction(int lineIndex)
        {
            //The parameter lineIndex indicates which line we are in in the original code
            int functionIndex = routine.Function.Count - 1;
            int basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            string aux = original[lineIndex].Substring(original[lineIndex].IndexOf('(') + 2); //Extracting the control elements
            aux = aux.Remove(aux.Length - 2);
            string[] tokens = aux.Split(' ');
            //aux is something similar to "i < 100"
            if (tokens.Length == 3) 
            {
                CondJumpInstruction(string.Join(" ", tokens[0], tokens[1], tokens[2]));
            }
            //aux is something similar to "i * a1 < 100 "
            else
            {
                string tempVariable1 = string.Empty;
                string tempVariable2;
                int i = 0;
                while (i < tokens.Length && (tokens[i + 1].Equals("*") || tokens[i + 1].Equals("-") || tokens[i + 1].Equals("+") || tokens[i + 1].Equals("/")))
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
                CondJumpInstruction(string.Join(" ", tempVariable1, tokens[i + 1], tokens[i + 2]));
            }

            //New Basic Block for the True Lane
            BasicBlockType trueBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); 
            trueBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            trueBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            trueBasicBlock.Successors.Value = string.Empty;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = trueBasicBlock.ID.Value;
            int instructionIndex = routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction.Count - 1;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction[instructionIndex].Value = string.Concat(
                routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction[instructionIndex].Value, trueBasicBlock.ID.Value);
            //Processing instructions between { } //Returns the new line index            
            lineIndex = ProcessInnerScope(lineIndex); 
            lineIndex++;
            int lastInnerTrueBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;

            int lastFalseBasicBlockIndex = -1;
            if (original[lineIndex].Contains("else"))
            {
                //New Basic Block for the False Lane
                BasicBlockType falseBasicBlock = routine.Function[functionIndex].BasicBlock.Append();
                falseBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
                falseBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
                falseBasicBlock.Successors.Value = string.Empty;
                routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = string.Join(" ",
                    routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value, falseBasicBlock.ID.Value);
                //Processing instructions between { } //Returns the new line index
                lineIndex = ProcessInnerScope(lineIndex); 
                lastFalseBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            }

            //New Basic Block for the intructions after the IF
            BasicBlockType newBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); 
            newBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newBasicBlock.Successors.Value = string.Empty;
            //Linking basic blocks
            if (lastFalseBasicBlockIndex != -1)
            {
                newBasicBlock.Predecessors.Value = string.Join(" ", routine.Function[functionIndex].BasicBlock[lastInnerTrueBasicBlockIndex].ID.Value,
                    routine.Function[functionIndex].BasicBlock[lastFalseBasicBlockIndex].ID.Value);
                if (routine.Function[functionIndex].BasicBlock[lastFalseBasicBlockIndex].Successors.Value.Length > 0)
                    routine.Function[functionIndex].BasicBlock[lastFalseBasicBlockIndex].Successors.Value = string.Join(" ",
                        newBasicBlock.ID.Value, routine.Function[functionIndex].BasicBlock[lastFalseBasicBlockIndex].Successors.Value);
                else
                    routine.Function[functionIndex].BasicBlock[lastFalseBasicBlockIndex].Successors.Value = newBasicBlock.ID.Value;
                UncondJumpInstruction(lastFalseBasicBlockIndex, newBasicBlock.ID.Value);
            }
            else
                newBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[lastInnerTrueBasicBlockIndex].ID.Value;
            if (routine.Function[functionIndex].BasicBlock[lastInnerTrueBasicBlockIndex].Successors.Value.Length > 0)
                routine.Function[functionIndex].BasicBlock[lastInnerTrueBasicBlockIndex].Successors.Value = string.Join(" ",
                    newBasicBlock.ID.Value, routine.Function[functionIndex].BasicBlock[lastInnerTrueBasicBlockIndex].Successors.Value);
            else
                routine.Function[functionIndex].BasicBlock[lastInnerTrueBasicBlockIndex].Successors.Value = newBasicBlock.ID.Value;
            

            return lineIndex;
        }
        private static int ForInstruction(int lineIndex)
        {
            //The parameter lineIndex indicates which line we are in in the original code
            int functionIndex = routine.Function.Count - 1;
            int basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            //Extracting the control elements ( i = 2 i < 10 i++ )
            string aux = original[lineIndex].Substring(original[lineIndex].IndexOf('(') + 2); 
            aux = aux.Remove(aux.Length - 2);
            string[] tokens = aux.Split(' ');
            //i = 2
            CopyInstruction(string.Join(" ", tokens[0], tokens[1], tokens[2]));
            
            //New Basic Block for the Conditional Jump
            BasicBlockType condJumpBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); 
            condJumpBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            condJumpBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = condJumpBasicBlock.ID.Value;
            //i < 10
            CondJumpInstruction(string.Join(" ", tokens[3], tokens[4], tokens[5]));
            basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;

            //New Basic Block for the Inner Scope
            BasicBlockType innerScopeBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); 
            innerScopeBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            innerScopeBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            innerScopeBasicBlock.Successors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Predecessors.Value = string.Join(" ",
                routine.Function[functionIndex].BasicBlock[basicBlockIndex].Predecessors.Value, innerScopeBasicBlock.ID.Value);
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = innerScopeBasicBlock.ID.Value;
            int instructionIndex = routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction.Count - 1;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction[instructionIndex].Value = string.Concat(
                routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction[instructionIndex].Value, innerScopeBasicBlock.ID.Value);
            //Processing instructions between { } //Returns the new line index //i++
            lineIndex = ProcessInnerScope(lineIndex, tokens[6]); 
            UncondJumpInstruction(routine.Function[functionIndex].BasicBlock.Count - 1, routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value);

            //New Basic Block for the Unconditional Jump
            BasicBlockType uncondJumpBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); 
            uncondJumpBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            uncondJumpBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = string.Join(" ",
                routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value, uncondJumpBasicBlock.ID.Value);
            basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;

            //New Basic Block for the intructions after the For Loop
            BasicBlockType newBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); 
            newBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            newBasicBlock.Successors.Value = string.Empty;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = newBasicBlock.ID.Value;
            UncondJumpInstruction(basicBlockIndex,newBasicBlock.ID.Value);

            return lineIndex;
        }
        private static int DoWhileInstruction(int lineIndex)
        {
            //The parameter lineIndex indicates which line we are in in the original code
            int functionIndex = routine.Function.Count - 1;
            int basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            //New Basic Block for the Inner Scope
            BasicBlockType innerScopeBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); 
            innerScopeBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            innerScopeBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = innerScopeBasicBlock.ID.Value;
            innerScopeBasicBlock.Successors.Value = string.Empty;
            int innerBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            //Processing instructions between { } //Returns the new line index
            lineIndex = ProcessInnerScope(lineIndex); 
            lineIndex++;
            basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;

            //New Basic Block for the Conditional Jump
            BasicBlockType condJumpBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); 
            condJumpBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            condJumpBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = condJumpBasicBlock.ID.Value;
            condJumpBasicBlock.Successors.Value = routine.Function[functionIndex].BasicBlock[innerBasicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[innerBasicBlockIndex].Predecessors.Value = string.Join(" ",
                routine.Function[functionIndex].BasicBlock[innerBasicBlockIndex].Predecessors.Value, condJumpBasicBlock.ID.Value);
            //Extracting the control elements
            string aux = original[lineIndex].Substring(original[lineIndex].IndexOf('(') + 2); 
            aux = aux.Remove(aux.Length - 2);
            string[] tokens = aux.Split(' ');
            //aux is something like i < 100
            if (tokens.Length == 3)
            {
                CondJumpInstruction(string.Join(" ",tokens[0], tokens[1], tokens[2]));
            }
            //aux is something like i * j < 100
            else
            {
                string tempVariable1 = string.Empty;
                string tempVariable2;
                int i = 0;
                while (i < tokens.Length && (tokens[i + 1].Equals("*") || tokens[i + 1].Equals("-") || tokens[i + 1].Equals("+") || tokens[i + 1].Equals("/")))
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
                CondJumpInstruction(string.Join(" ", tempVariable1, tokens[i + 1], tokens[i + 2]));
            }
            int instructionIndex = condJumpBasicBlock.Instruction.Count - 1;
            condJumpBasicBlock.Instruction[instructionIndex].Value = string.Concat(condJumpBasicBlock.Instruction[instructionIndex].Value,
                routine.Function[functionIndex].BasicBlock[innerBasicBlockIndex].ID.Value);
            basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;

            //New Basic Block for the intructions after the Do While Loop
            BasicBlockType newBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); 
            newBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = string.Join(" ",
                routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value, newBasicBlock.ID.Value);

            return lineIndex;
        }
        private static int WhileInstruction(int lineIndex)
        {
            //The parameter lineIndex indicates which line we are in in the original code
            int functionIndex = routine.Function.Count - 1;
            int basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            //Extracting the control elements
            string aux = original[lineIndex].Substring(original[lineIndex].IndexOf('(') + 2); 
            aux = aux.Remove(aux.Length - 2);
            string[] tokens = aux.Split(' ');

            //New Basic Block for the Conditional Jump
            BasicBlockType condJumpBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); 
            condJumpBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            condJumpBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = condJumpBasicBlock.ID.Value;
            condJumpBasicBlock.Successors.Value = string.Empty;
            //aux is something like i < 100
            if (tokens.Length == 3)
            {
                CondJumpInstruction(string.Join(" ", tokens[0], tokens[1], tokens[2]));
            }
            //aux is something like i * j < 100
            else
            {
                string tempVariable1 = string.Empty;
                string tempVariable2;
                int i = 0;
                while (i < tokens.Length && (tokens[i + 1].Equals("*") || tokens[i + 1].Equals("-") || tokens[i + 1].Equals("+") || tokens[i + 1].Equals("/")))
                {
                    if (tempVariable1.Length == 0)
                    {
                        tempVariable1 = string.Concat("t_", randNumbers.Next().ToString());
                        CreateLocalVariable(string.Concat("int ", tempVariable1));
                        FullAssignInstruction(string.Join(" ",tempVariable1, "=", tokens[i], tokens[i + 1], tokens[i + 2]));
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
                CondJumpInstruction(string.Join(" ",tempVariable1, tokens[i + 1], tokens[i + 2]));
            }
            basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;

            //New Basic Block for the Inner Scope
            BasicBlockType innerScopeBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); 
            innerScopeBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            innerScopeBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            innerScopeBasicBlock.Successors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Predecessors.Value = string.Join(" ",
                routine.Function[functionIndex].BasicBlock[basicBlockIndex].Predecessors.Value, innerScopeBasicBlock.ID.Value);
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = innerScopeBasicBlock.ID.Value;
            int instructionIndex = routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction.Count - 1;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction[instructionIndex].Value = string.Concat(
                routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction[instructionIndex].Value, innerScopeBasicBlock.ID.Value);
            //Processing instructions between { } //Returns the new line index
            lineIndex = ProcessInnerScope(lineIndex); 
            UncondJumpInstruction(routine.Function[functionIndex].BasicBlock.Count - 1, routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value);

            //New Basic Block for the Unconditional Jump
            BasicBlockType uncondJumpBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); 
            uncondJumpBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            uncondJumpBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = string.Join(" ",
                routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value, uncondJumpBasicBlock.ID.Value);
            basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;

            //New Basic Block for the intructions after the While Loop
            BasicBlockType newBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); 
            newBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = newBasicBlock.ID.Value;
            UncondJumpInstruction(basicBlockIndex, newBasicBlock.ID.Value);

            return lineIndex;
        }
        private static void CondJumpInstruction(string line)
        {
            //The parameter line is expected to have a content similar to "i &lt; a1"
            string[] tokens = line.Split(' ');
            int functionIndex = routine.Function.Count - 1;
            int basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            InstructionType newInstruction = routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction.Append();
            newInstruction.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newInstruction.StatementType.EnumerationValue = StatementTypeType.EnumValues.eConditionalJump;
            newInstruction.PolyRequired.Value = false;
            string refVars = string.Empty;
            //Adding i
            if (!GetIDVariable(tokens[0]).Equals(tokens[0]))
                refVars = string.Concat(refVars, GetIDVariable(tokens[0]));
            //Adding a1
            if (!GetIDVariable(tokens[2]).Equals(tokens[2]))
                if (refVars.Length != 0)
                    refVars = string.Concat(refVars, string.Concat(" ",GetIDVariable(tokens[2])));
                else
                    refVars = string.Concat(refVars, GetIDVariable(tokens[2]));
            newInstruction.RefVars.Value = refVars;
            newInstruction.Value = string.Join(" ", "if", GetValueVariable(tokens[0]), tokens[1], GetValueVariable(tokens[2]), "goto ");
        }
        private static void UncondJumpInstruction(int indexOwnerBasicBlock, string targetBasicBlockID)
        {
            //The parameter basicBlockID is expected to have a content similar to "ID_80D994CA-833A-4F05-99A9-EC3BD2DEA9EE"
            int functionIndex = routine.Function.Count - 1;
            InstructionType newInstruction = routine.Function[functionIndex].BasicBlock[indexOwnerBasicBlock].Instruction.Append();
            newInstruction.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newInstruction.StatementType.EnumerationValue = StatementTypeType.EnumValues.eUnconditionalJump;
            newInstruction.PolyRequired.Value = false;
            newInstruction.Value = string.Concat("goto ",targetBasicBlockID);
        }
        private static int ProcessInnerScope(int lineIndex, string operation = "")
        {
            int auxIndex = lineIndex + 1;
            if (original[auxIndex].Equals("{"))
            {
                auxIndex++;
                //while it is not the end of the inner scope
                while (!original[auxIndex].Equals("}"))
                {
                    auxIndex = CreateInstruction(auxIndex);
                    auxIndex++;
                }
            }
            else
            {
                auxIndex = CreateInstruction(auxIndex);
            }
            //In case we are processing an inner scope of a for loop. This operation would be
            //something like i++
            if (operation.Length > 0)
                PreFullAssignCopy(0, operation);
            return auxIndex;
        }
        private static string GetIDVariable(string globalID)
        {
            for (int i = 0; i < currentFunctionLocalVars.Variable.Count; i++)
            {
                if (currentFunctionLocalVars.Variable[i].GlobalID.Value == globalID)
                    return currentFunctionLocalVars.Variable[i].ID.Value;
            }
            return globalID;
        }
        private static string GetValueVariable(string globalID)
        {
            for (int i = 0; i < currentFunctionLocalVars.Variable.Count; i++)
            {
                if (currentFunctionLocalVars.Variable[i].GlobalID.Value == globalID)
                    return currentFunctionLocalVars.Variable[i].Value;
            }
            return globalID;
        }
        private static string GetIDFunction(string globalID)
        {
            
            for (int i = 0; i < routine.Function.Count; i++)
            {
                if (routine.Function[i].GlobalID.Value == globalID)
                    return routine.Function[i].ID.Value;
            }
            return globalID;
        }
    }
}

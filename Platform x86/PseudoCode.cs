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
        private static string[] original;
        private static Random randNumbers = new Random();
        
        public static XmlDocument GetTAC(string path2PC)
        {
            XmlDocument doc = new XmlDocument();
            ExchangeFormat.Exchange exchange = new Exchange(doc);
            routine = exchange.Routine.Append();
            routine.Description.Value = "Some routine";
            original = System.IO.File.ReadAllLines(path2PC);
            PreProc();
            Translate();
            FakeReturnInstruction(); //For the last function
            return doc;
        
        }
        private static void PreProc()
        {
            string[] operatorsPC = { "==", ">", "<", ">=", "<=" }; //Array of pseudo code operators
            string[] operatorsTAC = { "==", "&gt;", "&lt;", "&gt;=", "&lt;=" }; //Array of TAC operators
            string[] toReplacePC = { "_cdecl ", "*(_BYTE *)", "*(_DWORD *)", ",", ";", "signed ", "unsigned ", "(signed ", "(unsigned " }; //Array of unnecessary elements
            string[] toReplaceTAC = { "", "", "", "", "", "", "", "(", "(" };
            for (int i = 0; i < original.Length; i++)
            {
                for (int j = 0; j < toReplacePC.Length; j++) //Replacing unnecessary elements
                {
                    if (original[i].Contains(toReplacePC[j]))
                        original[i] = original[i].Replace(toReplacePC[j], toReplaceTAC[j]);
                }
                for (int j = 0; j < operatorsPC.Length; j++) //Replacing operators
                {
                    if (original[i].Contains(operatorsPC[j]))
                        original[i] = original[i].Replace(operatorsPC[j], operatorsTAC[j]);
                }
                if (original[i].Contains("//")) //Removing comments
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
                    foreach (string type in types) //Searching for types within each line
                    {
                        if (original[i].Contains(type))
                        {
                            if (original[i].Contains("_sub")) //Function
                            {
                                CreateFunction(original[i]);
                                translated = true;
                            }
                            else //Variable
                            {
                                CreateLocalVariable(original[i]);
                                translated = true;
                            }
                        }
                    }
                    if (translated == false) //Instruction
                        i = CreateInstruction(i); //The index will be updated accordingly to the number of lines consumed inside the function
                }
                i++;
            }
        }
        private static void CreateFunction(string line)
        {
            //The parameter line is expected to have a content similar to "int sub_401334(int a1 int a2)"
            if (routine.Function.Count > 0)
                FakeReturnInstruction();
            currentFunctionReturnType = line.Substring(0, line.IndexOf(" ")); //Extracting the return type
            line = line.Substring(line.IndexOf("sub_")); //Changing the line to "sub_401334(int a1 int a2)"
            string aux = line.Remove(line.IndexOf('(')); //Removing the parameters
            FunctionType newFunction = routine.Function.Append();
            newFunction.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newFunction.GlobalID.Value = aux;
            newFunction.CalledFrom.EnumerationValue = CalledFromType.EnumValues.eBoth; //Work on it
            currentFunctionLocalVars = newFunction.Local.Append();
            if (!line.Contains("()"))
            {
                string[] tokens = line.Split('('); //Separating parameters
                aux = tokens[1].Replace(")", "");
                CreateLocalVariable(aux); //Creating variables for the parameters
                tokens = aux.Split(' ');
                string inputVars = "";
                for (int i = 1; i < tokens.Length; i += 2) //Creating string of Input Variables
                {
                    if (inputVars.Length != 0)
                        inputVars = string.Concat(inputVars, string.Concat(" ", GetIDVariable(tokens[i]))); 
                    else
                        inputVars = GetIDVariable(tokens[i]);                    
                }
                routine.Function[routine.Function.Count - 1].RefInputVars.Value = inputVars;
            }
            BasicBlockType newBasicBlock = newFunction.BasicBlock.Append();
            newBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newBasicBlock.Successors.Value = "";
        }
        private static void CreateLocalVariable(string line)
        {
            //The parameter line is expected to have a content similar to "int a int b int c"
            bool pointer;
            string[] tokens = line.Split(' ');
            VariableType newVariable;
            for (int i = 0; i < tokens.Length - 1; i += 2)
            {
                if (tokens[i].IndexOf('*') != -1)
                    pointer = true;
                else
                    pointer = false;
                newVariable = currentFunctionLocalVars.Variable.Append();
                newVariable.ID.Value = string.Concat("ID_",Guid.NewGuid().ToString().ToUpper());
                newVariable.Value = string.Concat("v_",newVariable.ID.Value);
                newVariable.GlobalID.Value = tokens[i + 1];
                newVariable.Pointer.Value = pointer;
                newVariable.MemoryRegionSize.Value = 4; //Work on it               
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
            if (aux.Contains("sub_")) //aux is something like "return sub_401334(a)"
            {
                aux = aux.Substring(aux.IndexOf("sub_")); //sub_401334(a)
                aux = aux.Remove(aux.IndexOf('(')); //sub_401334
                CreateLocalVariable(string.Concat(string.Concat(currentFunctionReturnType, " "), aux));
                CallInstruction(original[lineIndex].Substring(original[lineIndex].IndexOf("sub_")));
                RetrieveInstruction(original[lineIndex].Substring(original[lineIndex].IndexOf("sub_")));
                ReturnInstruction(string.Concat("return ",aux));
            }
            else if (aux.Contains("+") || aux.Contains("-") || aux.Contains("*") || aux.Contains("/")) //aux is something like "return a1 + v5"
            {

            }
            else //aux is something like "return 0" or "return a1"
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
            if (tokens.Length > 1) //Real return
            {
                if (!GetIDVariable(tokens[1]).Equals(tokens[1]))
                    newInstruction.RefVars.Value = GetIDVariable(tokens[1]); //Adding "v3" to RefVars
                newInstruction.Value = string.Concat("return ", GetValueVariable(tokens[1]));
            }
            else //Fake return
                newInstruction.Value = "return";
        }
        private static void FakeReturnInstruction()
        {
            int functionIndex = routine.Function.Count - 1;
            int basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            BasicBlockType newBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); //New Basic Block for the Conditional Jump
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
            if (!line.Contains("()")) //Dealing with parameters
            {
                string aux = line.Substring(line.IndexOf('(') + 1); //Extracting parameters
                aux = aux.Replace(")", "");
                string[] tokens = aux.Split(' ');
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
            string[] tokens2 = line.Split('(');
            newInstruction = routine.Function[indexFunction].BasicBlock[indexBasicblock].Instruction.Append();
            newInstruction.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newInstruction.StatementType.EnumerationValue = StatementTypeType.EnumValues.eProcedural;
            newInstruction.PolyRequired.Value = false;
            newInstruction.Value = string.Concat(string.Concat("call ", GetIDFunction(tokens2[0])), string.Concat(" ", numParams));
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
            if (tokens.Length == 1) //aux is something like "v5++"
            {
                if (aux.IndexOf("++") != -1)
                {
                    aux = aux.Replace("++", "");
                    FullAssignInstruction(aux + " = " + aux + " + " + 1);
                }
                else // v5--
                {
                    aux = aux.Replace("--", "");
                    FullAssignInstruction(aux + " = " + aux + " - " + 1);
                }
            }
            else if (tokens.Length == 3 && ((aux.Contains("+=") || aux.Contains("-=")))) //aux is something like "v5 += v1"
            {
                tokens[1] = tokens[1].Remove(tokens[1].IndexOf('='), 1);
                FullAssignInstruction(tokens[0] + " = " + tokens[2] + " " + tokens[1] + " " + tokens[0]);
            }
            else //aux is something like "v5 = v1" or "v5 = v1 + v2" or "v5 = v1 += v2" or "v5 = v1 = v2"
            {
                if (tokens.Length == 3) //aux is "v5 = v1"
                {
                    CopyInstruction(aux);
                }
                else
                {
                    
                    int i = tokens.Length - 1;
                    while (i > 1)
                    {
                        if (tokens[i - 3].Equals("=") && ((tokens[i - 1].Equals("+") || tokens[i - 1].Equals("-") || tokens[i - 1].Equals("*") || tokens[i - 1].Equals("/")))) //aux is "v5 = v1 + v2"
                        {
                            string tempVariable = string.Concat("t_", randNumbers.Next().ToString());
                            CreateLocalVariable(string.Concat("int ", tempVariable));
                            FullAssignInstruction(tempVariable + " " + tokens[i - 3] + " " + tokens[i - 2] + " " + tokens[i - 1] + " " + tokens[i]);
                            CopyInstruction(tokens[i - 4] + " = " + tempVariable); //v5 = v1
                            i -= 4;
                        }
                        else if (tokens[i - 3].Equals("=") && ((tokens[i - 1].Equals("+=") || tokens[i - 1].Equals("-=")))) //aux is "v5 = v1 += v2"
                        {
                            tokens[i - 1] = tokens[i - 1].Replace("=", "");
                            FullAssignInstruction(tokens[i - 2] + " = " + tokens[i] + " " + tokens[i - 1] + " " + tokens[i - 2]); //v1 = v2 + v1
                            CopyInstruction(tokens[i - 4] + " " + tokens[i - 3] + " " + tokens[i - 2]); //v5 = v1
                            i -= 4;
                        }
                        else //aux is "v5 = v1 = v2"
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
            string refVars = "";
            if (!GetIDVariable(tokens[0]).Equals(tokens[0]))
                refVars = GetIDVariable(tokens[0]); //Adding v3
            if (!GetIDVariable(tokens[2]).Equals(tokens[2]))
                if (refVars.Length != 0)
                    if (!refVars.Contains(GetIDVariable(tokens[2])))
                        refVars = string.Concat(refVars, string.Concat(" ", GetIDVariable(tokens[2]))); //Adding a2
                else
                    refVars = GetIDVariable(tokens[2]); //Adding v3
            if (!GetIDVariable(tokens[4]).Equals(tokens[4]))
                if (refVars.Length != 0)
                    if (!refVars.Contains(GetIDVariable(tokens[4])))
                    refVars = string.Concat(refVars, string.Concat(" ", GetIDVariable(tokens[4]))); //Adding a1
                else
                    refVars = GetIDVariable(tokens[4]); //Adding v3
            newInstruction.RefVars.Value = refVars;
            newInstruction.Value = string.Concat(string.Concat(GetValueVariable(tokens[0]), string.Concat(" := ",
                string.Concat(GetValueVariable(tokens[2]), string.Concat(" ", string.Concat(tokens[3],
                string.Concat(" ", GetValueVariable(tokens[4]))))))));
        }
        private static void CopyInstruction(string input)
        {
            //The parameter "input" is expected to have a content similar to "v3 = 10" or "v3 = a1"
            string[] tokens = input.Split(' ');
            int functionIndex = routine.Function.Count - 1;
            int basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            InstructionType newInstruction = routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction.Append();
            newInstruction.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newInstruction.StatementType.EnumerationValue = StatementTypeType.EnumValues.eCopy;
            newInstruction.PolyRequired.Value = false;
            string refVars = "";
            if (!GetIDVariable(tokens[0]).Equals(tokens[0])) //Checking whether the token is a variable
                refVars = GetIDVariable(tokens[0]);
            if (!GetIDVariable(tokens[2]).Equals(tokens[2]))
                if (refVars.Length != 0)
                    refVars = string.Concat(refVars, string.Concat(" ",GetIDVariable(tokens[2])));
                else
                    refVars = GetIDVariable(tokens[2]);
            newInstruction.RefVars.Value = refVars;
            newInstruction.Value = string.Concat(GetValueVariable(tokens[0]), 
                string.Concat(" := ", GetValueVariable(tokens[2])));
        }
        private static int IfInstruction(int lineIndex)
        {
            //The parameter lineIndex indicates which line we are in in the original code
            int functionIndex = routine.Function.Count - 1;
            int basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            string aux = original[lineIndex].Substring(original[lineIndex].IndexOf('(') + 2); //Extracting the control elements
            aux = aux.Remove(aux.Length - 2);
            string[] tokens = aux.Split(' ');
            if (tokens.Length == 3)
            {
                CondJumpInstruction(string.Concat(tokens[0], string.Concat(" ", string.Concat(tokens[1],
                string.Concat(" ", tokens[2])))));
            }
            else
            {
                string tempVariable1 = "";
                string tempVariable2;
                int i = 0;
                while (i < tokens.Length && (tokens[i + 1].Equals("*") || tokens[i + 1].Equals("-") || tokens[i + 1].Equals("+") || tokens[i + 1].Equals("/")))
                {
                    if (tempVariable1.Length == 0)
                    {
                        tempVariable1 = string.Concat("t_", randNumbers.Next().ToString());
                        CreateLocalVariable(string.Concat("int ", tempVariable1));
                        FullAssignInstruction(string.Concat(tempVariable1, string.Concat(" = ", string.Concat(tokens[i],
                            string.Concat(" ", string.Concat(tokens[i + 1], string.Concat(" ", tokens[i + 2])))))));
                    }
                    else
                    {
                        tempVariable2 = string.Concat("t_", randNumbers.Next().ToString());
                        CreateLocalVariable(string.Concat("int ", tempVariable2));
                        FullAssignInstruction(string.Concat(tempVariable2, string.Concat(" = ", string.Concat(tempVariable1,
                            string.Concat(" ", string.Concat(tokens[i + 1], string.Concat(" ", tokens[i + 2])))))));
                        tempVariable1 = tempVariable2;
                    }
                    i += 2;
                }
                CondJumpInstruction(string.Concat(tempVariable1, string.Concat(" ", string.Concat(tokens[i+1],
                string.Concat(" ", tokens[i+2])))));
            }

            BasicBlockType trueBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); //New Basic Block for the Inner Scope
            trueBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            trueBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            trueBasicBlock.Successors.Value = ""; //Initialization
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = trueBasicBlock.ID.Value;
            int instructionIndex = routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction.Count - 1;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction[instructionIndex].Value = string.Concat(
                routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction[instructionIndex].Value, trueBasicBlock.ID.Value);
            int trueBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;          
            
            lineIndex = ProcessInnerScope(lineIndex); //Processing instructions between { } //Returns the new line index            
            lineIndex++;

            int falseBasicBlockIndex = -1;
            if (original[lineIndex].Contains("else"))
            {
                BasicBlockType falseBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); //New Basic Block for the Inner Scope
                falseBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
                falseBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
                falseBasicBlock.Successors.Value = ""; //Initialization
                routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = string.Concat(
                    routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value,string.Concat(
                    " ", falseBasicBlock.ID.Value));
                falseBasicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
                lineIndex = ProcessInnerScope(lineIndex); //Processing instructions between { } //Returns the new line index
            }

            BasicBlockType newBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); //New Basic Block for the intructions after the IF
            newBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            if (falseBasicBlockIndex != -1)
            {
                newBasicBlock.Predecessors.Value = string.Concat(routine.Function[functionIndex].BasicBlock[trueBasicBlockIndex].ID.Value,
                    string.Concat(" ", routine.Function[functionIndex].BasicBlock[falseBasicBlockIndex].ID.Value));
                if (routine.Function[functionIndex].BasicBlock[falseBasicBlockIndex].Successors.Value.Length > 0)
                    routine.Function[functionIndex].BasicBlock[falseBasicBlockIndex].Successors.Value = string.Concat(
                        newBasicBlock.ID.Value, string.Concat(" ", routine.Function[functionIndex].BasicBlock[falseBasicBlockIndex].Successors.Value));
                else
                    routine.Function[functionIndex].BasicBlock[falseBasicBlockIndex].Successors.Value = newBasicBlock.ID.Value;
                UncondJumpInstruction(falseBasicBlockIndex, newBasicBlock.ID.Value);
            }
            else
                newBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[trueBasicBlockIndex].ID.Value;
            if (routine.Function[functionIndex].BasicBlock[trueBasicBlockIndex].Successors.Value.Length > 0)
                routine.Function[functionIndex].BasicBlock[trueBasicBlockIndex].Successors.Value = string.Concat(
                    newBasicBlock.ID.Value, string.Concat(" ", routine.Function[functionIndex].BasicBlock[trueBasicBlockIndex].Successors.Value));
            else
                routine.Function[functionIndex].BasicBlock[trueBasicBlockIndex].Successors.Value = newBasicBlock.ID.Value;
            UncondJumpInstruction(trueBasicBlockIndex, newBasicBlock.ID.Value);
            

            return lineIndex;
        }
        private static int ForInstruction(int lineIndex)
        {
            //The parameter lineIndex indicates which line we are in in the original code
            int functionIndex = routine.Function.Count - 1;
            int basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            string aux = original[lineIndex].Substring(original[lineIndex].IndexOf('(') + 2); //Extracting the control elements
            aux = aux.Remove(aux.Length - 2);
            string[] tokens = aux.Split(' ');
            CopyInstruction(string.Concat(tokens[0],string.Concat(" ",string.Concat(tokens[1],string.Concat(" ",tokens[2]))))); // i = 2
            
            BasicBlockType condJumpBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); //New Basic Block for the Conditional Jump
            condJumpBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            condJumpBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = condJumpBasicBlock.ID.Value;
            CondJumpInstruction(string.Concat(tokens[3],string.Concat(" ",string.Concat(tokens[4],
                string.Concat(" ",tokens[5])))));
            basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;

            BasicBlockType innerScopeBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); //New Basic Block for the Inner Scope
            innerScopeBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            innerScopeBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            innerScopeBasicBlock.Successors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Predecessors.Value = string.Concat(
                routine.Function[functionIndex].BasicBlock[basicBlockIndex].Predecessors.Value,string.Concat(
                " ", innerScopeBasicBlock.ID.Value));
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = innerScopeBasicBlock.ID.Value;
            int instructionIndex = routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction.Count - 1;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction[instructionIndex].Value = string.Concat(
                routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction[instructionIndex].Value, innerScopeBasicBlock.ID.Value);
            lineIndex = ProcessInnerScope(lineIndex, tokens[6]); //Processing instructions between { } //Returns the new line index
            UncondJumpInstruction(routine.Function[functionIndex].BasicBlock.Count - 1,routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value);

            BasicBlockType uncondJumpBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); //New Basic Block for the Unconditional Jump
            uncondJumpBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            uncondJumpBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = string.Concat(
                routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value,string.Concat(
                " ", uncondJumpBasicBlock.ID.Value));
            basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;

            BasicBlockType newBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); //New Basic Block for the intructions after the For Loop
            newBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = newBasicBlock.ID.Value;
            UncondJumpInstruction(basicBlockIndex,newBasicBlock.ID.Value);

            return lineIndex;
        }
        private static int DoWhileInstruction(int lineIndex)
        {
            //The parameter lineIndex indicates which line we are in in the original code
            int functionIndex = routine.Function.Count - 1;
            int basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;            
            BasicBlockType innerScopeBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); //New Basic Block for the Inner Scope
            innerScopeBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            innerScopeBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = innerScopeBasicBlock.ID.Value;
            basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            lineIndex = ProcessInnerScope(lineIndex); //Processing instructions between { } //Returns the new line index
            lineIndex++;

            BasicBlockType condJumpBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); //New Basic Block for the Conditional Jump
            condJumpBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            condJumpBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = condJumpBasicBlock.ID.Value;
            condJumpBasicBlock.Successors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Predecessors.Value = string.Concat(
                routine.Function[functionIndex].BasicBlock[basicBlockIndex].Predecessors.Value, string.Concat(
                " ", condJumpBasicBlock.ID.Value));
            string aux = original[lineIndex].Substring(original[lineIndex].IndexOf('(') + 2); //Extracting the control elements
            aux = aux.Remove(aux.Length - 2);
            string[] tokens = aux.Split(' ');
            if (tokens.Length == 3)
            {
                CondJumpInstruction(string.Concat(tokens[0], string.Concat(" ", string.Concat(tokens[1],
                string.Concat(" ", tokens[2])))));
            }
            else
            {
                string tempVariable1 = "";
                string tempVariable2;
                int i = 0;
                while (i < tokens.Length && (tokens[i + 1].Equals("*") || tokens[i + 1].Equals("-") || tokens[i + 1].Equals("+") || tokens[i + 1].Equals("/")))
                {
                    if (tempVariable1.Length == 0)
                    {
                        tempVariable1 = string.Concat("t_", randNumbers.Next().ToString());
                        CreateLocalVariable(string.Concat("int ", tempVariable1));
                        FullAssignInstruction(string.Concat(tempVariable1, string.Concat(" = ", string.Concat(tokens[i],
                            string.Concat(" ", string.Concat(tokens[i + 1], string.Concat(" ", tokens[i + 2])))))));
                    }
                    else
                    {
                        tempVariable2 = string.Concat("t_", randNumbers.Next().ToString());
                        CreateLocalVariable(string.Concat("int ", tempVariable2));
                        FullAssignInstruction(string.Concat(tempVariable2, string.Concat(" = ", string.Concat(tempVariable1,
                            string.Concat(" ", string.Concat(tokens[i + 1], string.Concat(" ", tokens[i + 2])))))));
                        tempVariable1 = tempVariable2;
                    }
                    i += 2;
                }
                CondJumpInstruction(string.Concat(tempVariable1, string.Concat(" ", string.Concat(tokens[i + 1],
                string.Concat(" ", tokens[i + 2])))));
            }
            int instructionIndex = condJumpBasicBlock.Instruction.Count - 1;
            condJumpBasicBlock.Instruction[instructionIndex].Value = string.Concat(condJumpBasicBlock.Instruction[instructionIndex].Value,
                routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value);
            UncondJumpInstruction(basicBlockIndex, condJumpBasicBlock.ID.Value);
            basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;

            BasicBlockType uncondJumpBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); //New Basic Block for the Unconditional Jump
            uncondJumpBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            uncondJumpBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = string.Concat(
                routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value, string.Concat(
                " ", uncondJumpBasicBlock.ID.Value));
            basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;

            BasicBlockType newBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); //New Basic Block for the intructions after the Do While Loop
            newBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            newBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = newBasicBlock.ID.Value;
            UncondJumpInstruction(basicBlockIndex, newBasicBlock.ID.Value);

            return lineIndex;
        }
        private static int WhileInstruction(int lineIndex)
        {
            //The parameter lineIndex indicates which line we are in in the original code
            int functionIndex = routine.Function.Count - 1;
            int basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;
            string aux = original[lineIndex].Substring(original[lineIndex].IndexOf('(') + 2); //Extracting the control elements
            aux = aux.Remove(aux.Length - 2);
            string[] tokens = aux.Split(' ');
            BasicBlockType condJumpBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); //New Basic Block for the Conditional Jump
            condJumpBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            condJumpBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = condJumpBasicBlock.ID.Value;
            if (tokens.Length == 3)
            {
                CondJumpInstruction(string.Concat(tokens[0], string.Concat(" ", string.Concat(tokens[1],
                string.Concat(" ", tokens[2])))));
            }
            else
            {
                string tempVariable1 = "";
                string tempVariable2;
                int i = 0;
                while (i < tokens.Length && (tokens[i + 1].Equals("*") || tokens[i + 1].Equals("-") || tokens[i + 1].Equals("+") || tokens[i + 1].Equals("/")))
                {
                    if (tempVariable1.Length == 0)
                    {
                        tempVariable1 = string.Concat("t_", randNumbers.Next().ToString());
                        CreateLocalVariable(string.Concat("int ", tempVariable1));
                        FullAssignInstruction(string.Concat(tempVariable1, string.Concat(" = ", string.Concat(tokens[i],
                            string.Concat(" ", string.Concat(tokens[i + 1], string.Concat(" ", tokens[i + 2])))))));
                    }
                    else
                    {
                        tempVariable2 = string.Concat("t_", randNumbers.Next().ToString());
                        CreateLocalVariable(string.Concat("int ", tempVariable2));
                        FullAssignInstruction(string.Concat(tempVariable2, string.Concat(" = ", string.Concat(tempVariable1,
                            string.Concat(" ", string.Concat(tokens[i + 1], string.Concat(" ", tokens[i + 2])))))));
                        tempVariable1 = tempVariable2;
                    }
                    i += 2;
                }
                CondJumpInstruction(string.Concat(tempVariable1, string.Concat(" ", string.Concat(tokens[i + 1],
                string.Concat(" ", tokens[i + 2])))));
            }
            basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;

            BasicBlockType innerScopeBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); //New Basic Block for the Inner Scope
            innerScopeBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            innerScopeBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            innerScopeBasicBlock.Successors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Predecessors.Value = string.Concat(
                routine.Function[functionIndex].BasicBlock[basicBlockIndex].Predecessors.Value, string.Concat(
                " ", innerScopeBasicBlock.ID.Value));
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = innerScopeBasicBlock.ID.Value;
            int instructionIndex = routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction.Count - 1;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction[instructionIndex].Value = string.Concat(
                routine.Function[functionIndex].BasicBlock[basicBlockIndex].Instruction[instructionIndex].Value, innerScopeBasicBlock.ID.Value);
            lineIndex = ProcessInnerScope(lineIndex); //Processing instructions between { } //Returns the new line index
            UncondJumpInstruction(routine.Function[functionIndex].BasicBlock.Count - 1, routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value);

            BasicBlockType uncondJumpBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); //New Basic Block for the Unconditional Jump
            uncondJumpBasicBlock.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString().ToUpper());
            uncondJumpBasicBlock.Predecessors.Value = routine.Function[functionIndex].BasicBlock[basicBlockIndex].ID.Value;
            routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value = string.Concat(
                routine.Function[functionIndex].BasicBlock[basicBlockIndex].Successors.Value, string.Concat(
                " ", uncondJumpBasicBlock.ID.Value));
            basicBlockIndex = routine.Function[functionIndex].BasicBlock.Count - 1;

            BasicBlockType newBasicBlock = routine.Function[functionIndex].BasicBlock.Append(); //New Basic Block for the intructions after the While Loop
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
            string refVars = "";
            if (!GetIDVariable(tokens[0]).Equals(tokens[0]))
                refVars = string.Concat(refVars, GetIDVariable(tokens[0])); //Adding i
            if (!GetIDVariable(tokens[2]).Equals(tokens[2]))
                if (refVars.Length != 0)
                    refVars = string.Concat(refVars, string.Concat(" ",GetIDVariable(tokens[2]))); //Adding a1
                else
                    refVars = string.Concat(refVars, GetIDVariable(tokens[2])); //Adding i
            newInstruction.RefVars.Value = refVars;
            newInstruction.Value = string.Concat("if ", string.Concat(GetValueVariable(tokens[0]), string.Concat(" ",
                string.Concat(tokens[1], string.Concat(" ", string.Concat(GetValueVariable(tokens[2]), " goto "))))));
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

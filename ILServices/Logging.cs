using Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Services
{
    public static class Logging
    {
        private static object sync = new object();
        private static string pathToLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
        private static string filename_log = Path.Combine(pathToLog, string.Format("Obfuscation_Exception_{0:dd.MM.yyy}.log", DateTime.Now));
        private static string pathToCFG = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CFG");
        private static Dictionary<BasicBlock, String> basicBlocksCFG_Attributes = new Dictionary<BasicBlock, String>();

        /// <summary>
        /// Draws the current routine CFG
        /// </summary>
        /// <param name="routine">Routine to be written</param>
        public static void DrawCFG(Routine routine, string obfuscationPoint = "")
        {
            if (!Directory.Exists(pathToCFG))
                Directory.CreateDirectory(pathToCFG);
            StringBuilder sb = new StringBuilder();
            string filename_cfg = string.Empty;
            string basicBlockID = string.Empty;
            string variableID = string.Empty;
            string relOperator = string.Empty;
            string successorID = string.Empty;
            string edgeAttributes = string.Empty;
            foreach (Function func in routine.Functions)
            {
                filename_cfg = Path.Combine(pathToCFG, string.Format(string.Concat(func.globalID, "_CFG_{0}_{1:dd.MM.yyy}.cfg"), obfuscationPoint, DateTime.Now));
                sb.AppendLine("digraph{");
                foreach (BasicBlock bb in func.BasicBlocks)
                {
                    basicBlockID = string.Join("","\"", bb.ID.ToString().Substring(bb.ID.ToString().IndexOf('_') + 1, 8), "\"");
                    //Defining the attributes for the actual basic block
                    if (!basicBlocksCFG_Attributes.ContainsKey(bb))
                    {
                        string bbAttribute = string.Empty;
                        if (bb.Instructions.Last().statementType == Objects.Common.StatementType.ConditionalJump)
                        {
                            bbAttribute = string.Concat(basicBlockID, " [ shape=diamond");
                            if (bb.Involve == BasicBlock.InvolveInFakeCodeGeneration.Both
                                && !obfuscationPoint.Equals("CONST"))
                                bbAttribute = string.Concat(bbAttribute, ", fillcolor=red, style=filled ];");
                            else
                            {
                                switch (obfuscationPoint)
                                {
                                    case "CONST":
                                        bbAttribute = string.Concat(bbAttribute, ", fillcolor=green2, style=filled ];");
                                        break;
                                    case "MeshingUNC":
                                        bbAttribute = string.Concat(bbAttribute, ", fillcolor=cyan2, style=filled ];");
                                        break;
                                    case "MeshingCOND":
                                        bbAttribute = string.Concat(bbAttribute, ", fillcolor=yellow2, style=filled ];");
                                        break;
                                    case "CondJumps":
                                        bbAttribute = string.Concat(bbAttribute, ", fillcolor=gray90, style=filled ];");
                                        break;
                                }
                            }
                        }
                        else
                        {
                            if (bb.Involve == BasicBlock.InvolveInFakeCodeGeneration.Both
                                && !obfuscationPoint.Equals("CONST"))
                                bbAttribute = string.Concat(basicBlockID, " [ fillcolor=red, style=filled ];");
                            else
                            {
                                switch (obfuscationPoint)
                                {
                                    case "CONST":
                                        bbAttribute = string.Concat(basicBlockID, " [ fillcolor=green2, style=filled ];");
                                        break;
                                    case "MeshingUNC":
                                        bbAttribute = string.Concat(basicBlockID, " [ fillcolor=cyan2, style=filled ];");
                                        break;
                                    case "MeshingCOND":
                                        bbAttribute = string.Concat(basicBlockID, " [ fillcolor=yellow2, style=filled ];");
                                        break;
                                    case "CondJumps":
                                        bbAttribute = string.Concat(basicBlockID, " [ fillcolor=gray90, style=filled ];");
                                        break;
                                }
                            }
                        }                            
                        basicBlocksCFG_Attributes.Add(bb, bbAttribute);
                    }
                    else
                    {
                        //Updating the shape of conditional jumps created during Meshing using basic blocks analyzed previously
                        if (bb.Instructions.Last().statementType == Objects.Common.StatementType.ConditionalJump
                            && !basicBlocksCFG_Attributes[bb].Contains("diamond"))
                            basicBlocksCFG_Attributes[bb] = basicBlocksCFG_Attributes[bb].Replace(" [", " [ shape=diamond,");

                    }

                    //Creating the edges
                    bool trueBranchFilled = false;
                    foreach (BasicBlock successor in bb.getSuccessors)
                    {
                        successorID = string.Join("","\"", successor.ID.ToString().Substring(successor.ID.ToString().IndexOf('_') + 1, 8), "\"");                        
                        if (bb.getSuccessors.Count > 1)
                        {
                            variableID = bb.Instructions.Last().GetVarFromCondition().ID.Substring(0, 6);
                            switch (bb.Instructions.Last().GetRelopFromCondition())
                            {
                                case Instruction.RelationalOperationType.Smaller:
                                    relOperator = " < ";
                                    break;
                                case Instruction.RelationalOperationType.SmallerOrEquals:
                                    relOperator = " <= ";
                                    break;
                                case Instruction.RelationalOperationType.Greater:
                                    relOperator = " > ";
                                    break;
                                case Instruction.RelationalOperationType.GreaterOrEquals:
                                    relOperator = " >= ";
                                    break;
                                case Instruction.RelationalOperationType.Equals:
                                    relOperator = " == ";
                                    break;
                                case Instruction.RelationalOperationType.NotEquals:
                                    relOperator = " != ";
                                    break;
                            }
                            if (successor == bb.getSuccessors.First() && !trueBranchFilled)
                            {
                                if (bb.Instructions.Last().GetConstFromCondition().ToString().Length > 0)
                                    edgeAttributes = string.Join("", "[label=\"", variableID, relOperator,
                                        bb.Instructions.Last().GetConstFromCondition().ToString(), "\"]");
                                else
                                {
                                    string secVariableID = bb.Instructions.Last().GetRightVarFromCondition().ID.Substring(0, 6);
                                    edgeAttributes = string.Join("", "[label=\"", variableID, relOperator,
                                        secVariableID, "\"]");
                                }
                                trueBranchFilled = true;
                            }
                            else
                                edgeAttributes = string.Join("", "[label=\"false\"]");
                        }
                        sb.AppendLine(string.Join(" ",basicBlockID,"->",successorID,edgeAttributes,";"));
                        edgeAttributes = string.Empty;
                    }
                    sb.AppendLine(basicBlocksCFG_Attributes[bb]);
                }
                sb.AppendLine("}");
                File.WriteAllText(filename_cfg, sb.ToString());
                sb.Clear();
            }
        }
        
        /// <summary>
        /// Writes an exception to external textfile
        /// </summary>
        /// <param name="ex">Exception to be logged</param>
        public static void WriteException(Exception ex)
        {

            if (!Directory.Exists(pathToLog))
                Directory.CreateDirectory(pathToLog);
            string fullText = string.Format("\n\n\n[{0:dd.MM.yyy HH:mm:ss.fff}] [{1}.{2}()] {3}\r\n{4}",
            DateTime.Now, ex.TargetSite.DeclaringType, ex.TargetSite.Name, ex.Message, ex.ToString());
            lock (sync)
            {
                File.WriteAllText(filename_log, fullText);
            }
        }


        /// <summary>
        /// Writes a routine to external textfile
        /// </summary>
        /// <param name="routine">Routine to be written</param>
        /// <param name="filename_distinguisher">Filename distinguisher (e.g. algorithm abbreviation)</param>
        public static void WriteRoutine(Routine routine, string filename_distinguisher = "")
        {
            if (!Directory.Exists(pathToLog))
                Directory.CreateDirectory(pathToLog);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(routine.ToString());
            string filename_routine = Path.Combine(pathToLog, string.Format("Obfuscation_Routine_{0}_{1:dd.MM.yyy}.log", filename_distinguisher, DateTime.Now));
            File.WriteAllText(filename_routine, sb.ToString());
        }


        /// <summary>
        /// Writes a routine to external textfile
        /// </summary>
        /// <param name="routine">Routine to be written</param>
        /// <param name="filename_distinguisher">Filename distinguisher (e.g. algorithm abbreviation)</param>
        public static void WriteStatistics(Routine routine)
        {
            if (!Directory.Exists(pathToLog))
                Directory.CreateDirectory(pathToLog);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(" *** STATISTICS ***");
            sb.AppendLine("Generation date: " + DateTime.Now);
            sb.AppendLine(" ******************");
            sb.AppendLine();
            sb.AppendLine("Routine description: " + routine.description);
            sb.AppendLine("Total number of functions: " + routine.Functions.Count);
            
            int counter = 0;
            routine.Functions.ForEach(x => counter += x.BasicBlocks.Count);
            sb.AppendLine("Total number of basic blocks: " + counter);

            counter = 0;
            routine.Functions.ForEach(x => counter += x.BasicBlocks.Count(y => y.inFakeLane));
            sb.AppendLine("   - out of them in fake lanes (reached by invalid parameters only): " + counter);

            int total_instructions = 0;
            routine.Functions.ForEach(x => x.BasicBlocks.ForEach(y => total_instructions += y.Instructions.Count));
            sb.AppendLine("Total number of instructions: " + total_instructions);

            counter = 0;
            routine.Functions.ForEach(x => x.BasicBlocks.ForEach(y => counter += y.Instructions.Count(z => z.isFake)));
            sb.AppendLine("   - out of them fake: " + counter);
            sb.AppendLine("   - average FPO: " + Math.Round((double)total_instructions/(total_instructions-counter), 3));

            sb.AppendLine();
            foreach (Function func in routine.Functions)
            {
                int sum = func.GetFakeExitBasicBlock().Instructions.First().DeadVariables.Count;
                int filled = func.GetFakeExitBasicBlock().Instructions.First().DeadVariables.Count(x => x.Value == Variable.State.Filled);
                sb.AppendLine("Dead variables at the end (all / filled): " + sum + " / " + filled);
            }
            
            string filename_routine = Path.Combine(pathToLog, string.Format("Obfuscation_Statistics_{0:dd.MM.yyy}.log", DateTime.Now));
            File.WriteAllText(filename_routine, sb.ToString());
        }


        /// <summary>
        /// Writes a 'readable' routine (TAC text) to a log file
        /// </summary>
        /// <param name="routine">Routine to be logged</param>
        /// <param name="filename_distinguisher">Filename distinguisher (e.g. last used algorithm abbreviation)</param>
        public static void WriteReadableTAC(Routine routine, string filename_distinguisher = "")
        {
            if (!Directory.Exists(pathToLog))
                Directory.CreateDirectory(pathToLog);
            StringBuilder sb = new StringBuilder();
            foreach (Function func in routine.Functions)
            {
                Dictionary<string, string> ReadableVariables = new Dictionary<string, string>();
                Dictionary<string, string> ReadableBBLabels = new Dictionary<string, string>();
                int counter = 0;
                func.LocalVariables.ForEach(x => ReadableVariables.Add(x.ID, string.Concat(counter++)));
                counter = 0;
                func.BasicBlocks.ForEach(x => ReadableBBLabels.Add(x.ID, string.Concat("LABEL_", counter++)));

                sb.AppendLine("FUNCTION:");
                sb.AppendLine("  - Global ID: " + func.globalID);

                sb.AppendLine("  - Input parameters: " + func.LocalVariables.Count(x => x.kind == Variable.Kind.Input && !x.fake));
                func.LocalVariables.FindAll(x => x.kind == Variable.Kind.Input && !x.fake).ConvertAll(x => x.ID).ForEach
                    (x => sb.AppendLine(string.Concat("\t", "x_" + ReadableVariables[x], "\t", x)));
                
                sb.AppendLine("     - Fake: " + func.LocalVariables.Count(x => x.kind == Variable.Kind.Input && x.fake));
                func.LocalVariables.FindAll(x => x.kind == Variable.Kind.Input && x.fake).ForEach(
                    (x => sb.AppendLine(string.Concat("\t", "x_" + ReadableVariables[x.ID], "\t", x.ID,
                        " FixedMin: " + (x.fixedMin.HasValue ? x.fixedMin.Value.ToString() : "N/A") +
                        " FixedMax: " + (x.fixedMax.HasValue ? x.fixedMax.Value.ToString() : "N/A") +
                        " FixedValue: " + (string.IsNullOrEmpty(x.fixedValue) ? "N/A" : x.fixedValue)))));

                sb.AppendLine("  - Output parameters: " + func.LocalVariables.Count(x => x.kind == Variable.Kind.Output));
                func.LocalVariables.FindAll(x => x.kind == Variable.Kind.Output).ConvertAll(x => x.ID).ForEach
                    (x => sb.AppendLine(string.Concat("\t", "x_" + ReadableVariables[x], "\t", x)));

                sb.AppendLine("  - Local parameters: " + func.LocalVariables.Count(x => x.kind == Variable.Kind.Local));
                func.LocalVariables.FindAll(x => x.kind == Variable.Kind.Local).ConvertAll(x => x.ID).ForEach
                   (x => sb.AppendLine(string.Concat("\t", "x_" + ReadableVariables[x], "\t", x)));

                sb.AppendLine("\n*****************************************************************************************\n");

                StringBuilder sb_instructions = new StringBuilder();

                Traversal.ReorderBasicBlocks(func);
          
                BasicBlock prev = func.BasicBlocks.First();
                foreach (BasicBlock bb in func.BasicBlocks)
                {
                    /* The "fake exit block" should not be written. */
                    if (bb.getSuccessors.Count == 0)
                        continue;

                    if (bb.getPredecessors.Count() > 1 || ( bb.getPredecessors.Count() == 1 && 
                        bb.getPredecessors.First() != prev ) )
                    {
                         sb_instructions.AppendLine(ReadableBBLabels[bb.ID] + ":");
                    }
                    bb.Instructions.ForEach(x => sb_instructions.AppendLine("\t" + x.TACtext));// + " | " + x.DeadVariables.Count));
                    prev = bb;
                }                

                ReadableVariables.Keys.ToList().ForEach(x => sb_instructions.Replace(x, ReadableVariables[x]));
                ReadableBBLabels.Keys.ToList().ForEach(x => sb_instructions.Replace(x, ReadableBBLabels[x]));

                sb.AppendLine();
                sb.AppendLine(sb_instructions.ToString());
                sb.AppendLine("\n*****************************************************************************************\n");
            }

            string filename_routine = Path.Combine(pathToLog, string.Format("Obfuscation_Readable_{0}_{1:dd.MM.yyy}.log", filename_distinguisher, DateTime.Now));
            File.WriteAllText(filename_routine, sb.ToString());
        }


        /// <summary>
        /// Writes the content of string variable to a log file
        /// </summary>
        /// <param name="content">String variable with content to be logged</param>
        /// <param name="filename_distinguisher">Filename distinguisher (e.g. last used algorithm abbreviation)</param>
        public static void WriteTextFile(string content, string filename_distinguisher = "")
        {
            if (!Directory.Exists(pathToLog))
                Directory.CreateDirectory(pathToLog);
            string filename_routine = Path.Combine(pathToLog, string.Format("Obfuscation_Text_{0}_{1:dd.MM.yyy}.log", filename_distinguisher, DateTime.Now));
            File.WriteAllText(filename_routine, content);
        }

        /// <summary>
        /// Writes various complexity metrics about the given routine
        /// </summary>
        /// <param name="routine">Routine to be logged</param>
        /// <param name="filename_distinguisher">Filename distinguisher (e.g. last used algorithm abbreviation)</param>
        public static void WriteComplexityMetrics(Routine routine, string filename_distinguisher = "")
        {
            if (!Directory.Exists(pathToLog))
                Directory.CreateDirectory(pathToLog);
            StringBuilder sb = new StringBuilder();
            foreach (Function func in routine.Functions)
            {
                //For McCabe's Metric
                int edges = 0;
                
                //For Halstead's Metric
                List<string> operators = new List<string>();
                List<string> operands = new List<string>();

                //For Elshoff's Metric
                List<string> bbReferencedVars = new List<string>();
                List<string> funcReferencedVars = new List<string>();
                List<string> bbDefinedVars = new List<string>();
                List<string> funcDefinedVars = new List<string>();
                int dataFlowComplexity = 0;

                //For McClure's Metric
                int numComparisons = 0;
                List<string> controlVariables = new List<string>();                

                foreach (BasicBlock bb in func.BasicBlocks)
                {
                    edges += bb.getSuccessors.Count;
                    foreach (Instruction ins in bb.Instructions)
                    {
                        switch (ins.statementType)
                        {
                            case Objects.Common.StatementType.ConditionalJump:                                
                                operators.Add("if");
                                operands.Add(ins.GetVarFromCondition().ID);
                                bbReferencedVars.Add(ins.GetVarFromCondition().ID);
                                controlVariables.Add(ins.GetVarFromCondition().ID);
                                numComparisons++;
                                switch (ins.GetRelopFromCondition())
                                {
                                    case Instruction.RelationalOperationType.Equals:
                                        operators.Add("==");
                                        break;
                                    case Instruction.RelationalOperationType.Greater:
                                        operators.Add(">");
                                        break;
                                    case Instruction.RelationalOperationType.GreaterOrEquals:
                                        operators.Add(">=");
                                        break;
                                    case Instruction.RelationalOperationType.NotEquals:
                                        operators.Add("!=");
                                        break;
                                    case Instruction.RelationalOperationType.Smaller:
                                        operators.Add("<");
                                        break;
                                    case Instruction.RelationalOperationType.SmallerOrEquals:
                                        operators.Add("<=");
                                        break;
                                }
                                if (ins.GetConstFromCondition().ToString().Length > 0)
                                    operands.Add(ins.GetConstFromCondition().ToString());
                                else
                                {
                                    operands.Add(ins.GetRightVarFromCondition().ID);
                                    bbReferencedVars.Add(ins.GetRightVarFromCondition().ID);
                                }
                                operators.Add("goto");
                                operands.Add(ins.GetTrueSucc().ID);
                                break;                                
                            case Objects.Common.StatementType.Copy:
                            case Objects.Common.StatementType.PointerAssignment:
                                operands.Add(ins.RefVariables.First().ID);
                                bbDefinedVars.Add(ins.RefVariables.First().ID);
                                operators.Add(":=");
                                if (ins.RefVariables.Count > 1)
                                {
                                    operands.Add(ins.RefVariables.Last().ID);
                                    bbReferencedVars.Add(ins.RefVariables.Last().ID);
                                }
                                else
                                    operands.Add(ins.TACtext.Split(' ')[2]);
                                break;
                            case Objects.Common.StatementType.FullAssignment:
                                operands.Add(ins.RefVariables.First().ID);
                                bbDefinedVars.Add(ins.RefVariables.First().ID);
                                operators.Add(":=");
                                operands.Add(ins.RefVariables[1].ID);
                                bbReferencedVars.Add(ins.RefVariables[1].ID);
                                operators.Add(ins.TACtext.Split(' ')[3]);
                                if (ins.RefVariables.Count > 2)
                                {
                                    operands.Add(ins.RefVariables.Last().ID);
                                    bbReferencedVars.Add(ins.RefVariables.Last().ID);
                                }
                                else
                                    operands.Add(ins.TACtext.Split(' ')[4]);
                                break;
                            case Objects.Common.StatementType.Procedural:
                                if (ins.TACtext.Contains("call"))
                                {
                                    operators.Add("call");
                                    operands.Add(ins.TACtext.Split(' ')[1]);
                                    operands.Add(ins.TACtext.Split(' ')[2]);
                                }
                                else if (ins.TACtext.Contains("param"))
                                {
                                    operators.Add("param");
                                    if (ins.RefVariables.Count > 0)
                                    {
                                        operands.Add(ins.RefVariables.First().ID);
                                        bbReferencedVars.Add(ins.RefVariables.First().ID);
                                    }
                                    else
                                        operands.Add(ins.TACtext.Split(' ')[1]);
                                }
                                else if (ins.TACtext.Contains("return"))
                                {
                                    operators.Add("return");
                                    if (ins.TACtext.Split(' ').Length > 1)
                                    {
                                        if (ins.RefVariables.Count > 0)
                                        {
                                            operands.Add(ins.RefVariables.First().ID);
                                            bbReferencedVars.Add(ins.RefVariables.First().ID);
                                        }
                                        else
                                            operands.Add(ins.TACtext.Split(' ')[1]);
                                    }
                                }
                                break;
                            case Objects.Common.StatementType.UnconditionalJump:
                                operators.Add("goto");
                                operands.Add(ins.TACtext.Split(' ')[1]);
                                break;
                        }
                    }
                    bbDefinedVars = bbDefinedVars.Distinct().ToList();
                    bbReferencedVars = bbReferencedVars.Distinct().ToList();
                    foreach (string definedVar in bbDefinedVars)
                        bbReferencedVars.Remove(definedVar);
                    bbReferencedVars = bbReferencedVars.Distinct().ToList();
                    dataFlowComplexity += bbReferencedVars.Count;
                    funcDefinedVars.AddRange(bbDefinedVars);
                    funcReferencedVars.AddRange(bbReferencedVars);
                    bbDefinedVars.Clear();
                    bbReferencedVars.Clear();
                }

                sb.AppendLine("FUNCTION:");
                sb.AppendLine("  - Global ID: " + func.globalID);                

                //Control Flow Complexity (McCabe's Metric)
                int nodes = func.BasicBlocks.Count;
                int controlFlowComplexity = edges - nodes + 2;
                sb.AppendLine("Control Flow Complexity (McCabe's Metric): " + controlFlowComplexity + " distinct paths");
                
                //Language Complexity (Halstead's Metric)
                int totalOperators = operators.Count;
                int totalOperands = operands.Count;
                operators = operators.Distinct().ToList();
                operands = operands.Distinct().ToList();
                int uniqueOperators = operators.Count;
                int uniqueOperands = operands.Count;
                int progLenght = totalOperators + totalOperands;
                int progVocabulary = uniqueOperators + uniqueOperands;
                double volume = progLenght * Math.Log(progVocabulary, 2);
                double difficulty = (uniqueOperators / 2) * (totalOperands / uniqueOperands);
                double effort = volume * difficulty;
                sb.AppendLine("\nLanguage Complexity (Halstead's Metric)");
                sb.AppendLine("  Program Vocabulary: " + progVocabulary + " unique operators and operands");
                sb.AppendLine("  Program Length: " + progLenght + " references to operators and operands");                
                sb.AppendLine("  Volume: ~ " + Convert.ToInt32(volume) + " mathematical bits");
                sb.AppendLine("  Difficulty to understand: " + difficulty);
                sb.AppendLine("  Effort to implement: " + effort.ToString("F"));
                sb.AppendLine("  Time to implement: " + (effort / 18).ToString("F") + " seconds");

                //Data Flow Complexity (Elshoff's Metric)                
                sb.AppendLine("\nData Flow Complexity (Elshoff's Metric): " + dataFlowComplexity);

                //Oviedo's Metric
                sb.AppendLine("\nOviedo's Metric: " + (controlFlowComplexity + dataFlowComplexity));

                //Decisional Complexity (McClure's Metric)
                controlVariables = controlVariables.Distinct().ToList();
                int decisionalComplexity = numComparisons + controlVariables.Count;
                sb.AppendLine("\nDecisional Complexity (McClure's Metric): " + decisionalComplexity);

                //Data Complexity (Chapin's Metric)
                funcDefinedVars = funcDefinedVars.Distinct().ToList();
                funcReferencedVars = funcReferencedVars.Distinct().ToList();
                int p = func.LocalVariables.FindAll(x => x.kind == Variable.Kind.Input).Count;
                int m = funcDefinedVars.Count;
                int c = controlVariables.Count;
                int t = func.LocalVariables.FindAll(x => !funcDefinedVars.Contains(x.ID) && !funcReferencedVars.Contains(x.ID)).Count;
                double dataComplexity = p + 2 * m + 3 * c + 0.5 * t;
                sb.AppendLine("\nData Complexity (Chapin's Metric): " + dataComplexity);

                sb.AppendLine("\n*****************************************************************************************\n");
            }

            string filename_routine = Path.Combine(pathToLog, string.Format("Obfuscation_Complexity_{0}_{1:dd.MM.yyy}.log", filename_distinguisher, DateTime.Now));
            File.WriteAllText(filename_routine, sb.ToString());
        }
    }
}


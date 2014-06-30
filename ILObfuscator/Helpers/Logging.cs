using Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Obfuscator
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
                    basicBlockID = string.Concat("\"", string.Concat(bb.ID.ToString().Substring(bb.ID.ToString().IndexOf('_') + 1, 8), "\""));
                    
                    //Defining the attributes for the actual basic block
                    if (!basicBlocksCFG_Attributes.ContainsKey(bb))
                    {
                        string bbAttribute = string.Empty;
                        if (bb.Instructions.Last().statementType == ExchangeFormat.StatementTypeType.EnumValues.eConditionalJump)
                        {
                            bbAttribute = string.Concat(basicBlockID, " [ shape=diamond");
                            switch (obfuscationPoint)
                            {
                                case "CONST":
                                    bbAttribute = string.Concat(bbAttribute, ", fillcolor=yellow, style=filled ];");
                                    break;
                                case "MeshingUNC":
                                    bbAttribute = string.Concat(bbAttribute, ", fillcolor=aquamarine, style=filled ];");
                                    break;
                                case "MeshingCOND":
                                    bbAttribute = string.Concat(bbAttribute, ", fillcolor=green, style=filled ];");
                                    break;
                                case "CondJumps":
                                    bbAttribute = string.Concat(bbAttribute, " ];");
                                    break;
                            }
                        }
                        else if (bb.Involve == BasicBlock.InvolveInFakeCodeGeneration.OriginalVariablesOnly)
                            bbAttribute = string.Concat(basicBlockID, " [ fillcolor=red, style=filled ];");
                        else
                        {
                            switch (obfuscationPoint)
                            {
                                case "CONST":
                                    bbAttribute = string.Concat(basicBlockID, " [ fillcolor=yellow, style=filled ];");
                                    break;
                                case "MeshingUNC":
                                    bbAttribute = string.Concat(basicBlockID, " [ fillcolor=aquamarine, style=filled ];");
                                    break;
                                case "MeshingCOND":
                                    bbAttribute = string.Concat(basicBlockID, " [ fillcolor=green, style=filled ];");
                                    break;
                            }
                        }
                        basicBlocksCFG_Attributes.Add(bb, bbAttribute);
                    }
                    else
                    {
                        //Updating the shape of conditional jumps created during Meshing using basic blocks analyzed previously
                        if (bb.Instructions.Last().statementType == ExchangeFormat.StatementTypeType.EnumValues.eConditionalJump
                            && !basicBlocksCFG_Attributes[bb].Contains("diamond"))
                            basicBlocksCFG_Attributes[bb] = basicBlocksCFG_Attributes[bb].Replace(" [", " [ shape=diamond,");

                    }

                    //Creating the edges
                    foreach (BasicBlock successor in bb.getSuccessors)
                    {
                        successorID = string.Concat("\"", string.Concat(successor.ID.ToString().Substring(successor.ID.ToString().IndexOf('_') + 1, 8), "\""));
                        if (bb.getSuccessors.Count > 1)
                        {
                            variableID = bb.Instructions.Last().GetVarFromCondition().ID.Substring(
                                bb.Instructions.Last().GetVarFromCondition().ID.IndexOf('_') + 1, 3);
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
                            if (successor == bb.getSuccessors.First())
                                edgeAttributes = string.Concat("[label=\"true - ",string.Concat(variableID,string.Concat(relOperator,
                                    string.Concat(bb.Instructions.Last().GetConstFromCondition().ToString(),"\"]"))));
                            else
                                edgeAttributes = "[label=\"false\"]";
                        }
                        sb.AppendLine(string.Concat(basicBlockID,string.Concat(" -> ",string.Concat(successorID,string.Concat(
                            edgeAttributes,";")))));
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
            routine.Functions.ForEach(x => counter += x.BasicBlocks.Count(y => y.dead));
            sb.AppendLine("   - out of them dead: " + counter);

            int total_instructions = 0;
            routine.Functions.ForEach(x => x.BasicBlocks.ForEach(y => total_instructions += y.Instructions.Count));
            sb.AppendLine("Total number of instructions: " + total_instructions);

            counter = 0;
            routine.Functions.ForEach(x => x.BasicBlocks.ForEach(y => counter += y.Instructions.Count(z => z.isFake)));
            sb.AppendLine("   - out of them fake: " + counter);
            sb.AppendLine("   - FPO number in settings: " + Common.FPO);
            sb.AppendLine("   - real FPO after obfuscation: " + Math.Round((double)total_instructions/(total_instructions-counter), 3));

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
                    bb.Instructions.ForEach(x => sb_instructions.AppendLine("\t" + x.TACtext));
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

    }
}


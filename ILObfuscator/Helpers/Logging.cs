using Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obfuscator
{
    public static class Logging
    {
        private static object sync = new object();
        private static string pathToLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
        private static string filename_log = Path.Combine(pathToLog, string.Format("Obfuscation_Exception_{0:dd.MM.yyy}.log", DateTime.Now));
        
        
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
            
            string filename_routine = Path.Combine(pathToLog, string.Format("Obfuscation_Statistics_{0:dd.MM.yyy}.log", DateTime.Now));
            File.WriteAllText(filename_routine, sb.ToString());
        }
    }
}


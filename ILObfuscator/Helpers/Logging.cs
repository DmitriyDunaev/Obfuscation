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
    }
}


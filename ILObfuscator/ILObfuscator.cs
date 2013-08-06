using ExchangeFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obfuscator
{
    public class ILObfuscator
    {
        public void Obfuscate(Exchange exch)
        {
            Routine routine = new Routine(exch);
            Console.WriteLine("\nValidating data in Routine. . .");
            routine.Validate();
            Console.WriteLine("Routine validation has passed successfully.\n\n");
            foreach (Function func in routine.Functions)
                DataAnalysis.DeadVarsAlgortihm(func);
            routine.Validate();
        }
    }
}

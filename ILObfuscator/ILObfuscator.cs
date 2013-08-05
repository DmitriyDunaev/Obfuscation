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
            routine.Validate();
            foreach (Function func in routine.Functions)
                DataAnalysis.DeadVarsAlgortihm(func);
            routine.Validate();
        }
    }
}

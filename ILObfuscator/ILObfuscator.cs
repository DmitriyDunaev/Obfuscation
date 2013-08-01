using ExchangeFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILObfuscator
{
    public class ILObfuscator
    {
        public void Obfuscate(Exchange exch)
        {
            Routine routine = new Routine(exch);
            foreach (Function func in routine.Functions)
                DataAnalysis.SetAllVariablesAsDead(func);

        }
    }
}

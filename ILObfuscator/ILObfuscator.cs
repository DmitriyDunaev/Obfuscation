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
            Console.WriteLine("\nValidating data in Routine. . .\n");
            routine.Validate();
            Console.WriteLine("Loading initial data: routine validation has passed successfully.\n\n");
            foreach (Function func in routine.Functions)
                DataAnalysis.DeadVarsAlgortihm(func);
            routine.Validate();
            Console.WriteLine("Dead variables algorithm: routine validation has passed successfully.\n\n");
            Instruction inst = new Instruction(StatementTypeType.EnumValues.eNoOperation);
            Meshing.MeshingAlgorithm(routine.Functions[1]); // Testing the function in the second function of the routine.
            routine.Validate();
            Console.WriteLine("Meshing algorithm: routine validation has passed successfully..\n\n");
        }
    }
}

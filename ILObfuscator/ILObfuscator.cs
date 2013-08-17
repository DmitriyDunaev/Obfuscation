using ExchangeFormat;
using Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obfuscator
{
    public static class ILObfuscator
    {
        public static void Obfuscate(Exchange exch)
        {
            Routine routine = new Routine(exch);
            Console.WriteLine("\nValidating data in Routine. . .\n");
            routine.Validate();
            Console.WriteLine("Loading initial data: routine validation has passed successfully.\n\n");

            ConstCoverage.CoverConsts(routine);
            routine.Validate();
            Console.WriteLine("Const covering algorithm: routine validation has passed successfully.\n\n");

            Instruction inst = new Instruction(StatementTypeType.EnumValues.eNoOperation, routine.Functions[1].BasicBlocks[0]);

            routine.Functions[1].BasicBlocks[0].Instructions.Add(inst);
            routine.Functions[1].BasicBlocks[0].Instructions.Last().MakeConditionalJump(routine.Functions[1].LocalVariables[1], 55, Instruction.RelationalOperationType.Less, routine.Functions[1].BasicBlocks[3]);

            Meshing.MeshingAlgorithm(routine);
            routine.Validate();
            Console.WriteLine("Meshing algorithm: routine validation has passed successfully..\n\n");

            foreach (Function func in routine.Functions)
                DataAnalysis.DeadVarsAlgortihm(func);
            routine.Validate();
            Console.WriteLine("Dead variables algorithm: routine validation has passed successfully.\n\n");

            foreach (Function func in routine.Functions)
                FakeCode.GenerateNoOperations(func);
            routine.Validate();
            Console.WriteLine("Generation of NoOperations: routine validation has passed successfully.\n\n");
        }
    }
}

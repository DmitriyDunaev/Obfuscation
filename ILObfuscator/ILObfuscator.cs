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
        public static void Obfuscate(ref Exchange exch)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\tINTERMEDIATE LEVEL OBFUSCATION\n");
            Console.ResetColor();

            Console.Write("Converting Exchange to Routine class");
            Routine routine = (Routine)exch;
            PrintSuccess();

            Console.Write("First validation of the routine");
            routine.Validate();
            PrintSuccess();

            Logging.WriteReadableTAC(routine);

            routine.Functions[0].NewFakeInputParameter(11, 55);
            routine.Functions[0].NewFakeInputParameter(22, 155);
            routine.Functions[0].NewFakeInputParameter(33, 545);

            routine.Functions[1].NewFakeInputParameter(11, 55);
            routine.Functions[1].NewFakeInputParameter(22, 155);
            routine.Functions[1].NewFakeInputParameter(33, 545);
            Variable var = Randomizer.FakeInputParameter(routine.Functions[1]);

            Console.Write("Constants covering algorithm");
            ConstCoverage.CoverConstants(routine);
            routine.Validate();
            PrintSuccess();

            Console.Write("Meshing algorithm: Unconditional Jumps");
            Meshing.MeshUnconditionals(routine);
            routine.Validate();
            PrintSuccess();

            Console.Write("Meshing algorithm: Conditional Jumps");
            Meshing.MeshConditionals(routine);
            routine.Validate();
            PrintSuccess();

            Console.Write("Generation of fake NOP instructions");
            foreach (Function func in routine.Functions)
                FakeCode.GenerateNoOperations(func);
            routine.Validate();
            PrintSuccess();
            Logging.WriteRoutine(routine, "NoOpersGeneration");

            Console.Write("Running data analysis");
            foreach (Function func in routine.Functions)
            {
                DataAnalysis.DeadVarsAlgortihm(func);
                DataAnalysis.GatherBasicBlockInfo(func);
            }
            routine.Validate();
            PrintSuccess();

            Console.Write("Generation of fake instructions from NOPs");
            foreach (Function func in routine.Functions)
                FakeCode.GenerateFakeInstructions(func);
            Logging.WriteRoutine(routine, "FakeIns");
            foreach (Function func in routine.Functions)
                FakeCode.GenerateConditionalJumps(func);
            Logging.WriteRoutine(routine, "CondJumps");
            routine.Validate();
            PrintSuccess();

            //Logging.WriteReadableTAC(routine);

            Console.Write("Collecting and writing statistics");
            Logging.WriteStatistics(routine);
            PrintSuccess();

            Console.Write("Converting Routine to Exchange class");
            exch = (Exchange)routine;
            PrintSuccess();
        }



        public static void PrintSuccess()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            if (Console.CursorLeft % 2 != 0)
                Console.Write(" ");
            for (int i = Console.CursorLeft; i < 56; i += 2)
                Console.Write(". ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("COMPLETED\n");
            Console.ResetColor();
        }
    }
}

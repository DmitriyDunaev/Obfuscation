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
        private static void Obfuscation(Routine routine)
        {
            foreach (Function func in routine.Functions)
            {
                int paramnumber = Randomizer.SingleNumber(Common.FakeParamMin, Common.FakeParamMax);
                for (int i = 0; i < paramnumber; i++)
                {
                    List<int> borders = Randomizer.MultipleNumbers(2, Common.GlobalMinValue, Common.GlobalMaxValue, false, true);
                    func.NewFakeInputParameter(borders.First(), borders.Last());
                }
            }

            Console.Write("Constants covering algorithm");
            ConstCoverage.CoverConstants(routine);
            routine.Validate();
            PrintSuccess();

            Logging.WriteReadableTAC(routine, "CONST");

            Console.Write("Meshing algorithm: Unconditional Jumps");
            Meshing.MeshUnconditionals(routine);
            routine.Validate();
            PrintSuccess();

            Logging.WriteReadableTAC(routine, "MeshingUNC");

            Console.Write("Meshing algorithm: Conditional Jumps");
            Meshing.MeshConditionals(routine);
            routine.Validate();
            PrintSuccess();

            Logging.WriteReadableTAC(routine, "MeshingCOND");

            Console.Write("Generation of fake NOP instructions");
            foreach (Function func in routine.Functions)
                FakeCode.GenerateNoOperations(func);
            routine.Validate();
            PrintSuccess();
            Logging.WriteRoutine(routine, "NoOpersGeneration");

            Logging.WriteReadableTAC(routine, "FakeNOPs");

            Console.Write("Running data analysis");
            foreach (Function func in routine.Functions)
                DataAnalysis.DeadVarsAlgortihm(func);
            DataAnalysis.GatherBasicBlockInfo(routine);
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

            Logging.WriteReadableTAC(routine, "FakeInstrFromNOPs");
        }

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

            Logging.WriteReadableTAC(routine, "Imported");

            int NumberOfRuns = 0;
            bool Success;
            do
            {
                Success = true;
                try
                {
                    Obfuscation(routine);
                }
                catch (ObfuscatorException)
                {
                    Success = false;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" . . . . . . . FAILED\n");
                    Console.ResetColor();
                    routine = (Routine)exch;
                }
                NumberOfRuns++;
            } while (!Success && NumberOfRuns < Common.MaxNumberOfRuns);

            Console.Write("Writing readable TAC");
            Logging.WriteReadableTAC(routine);
            routine.Validate();
            PrintSuccess();

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

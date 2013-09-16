﻿using ExchangeFormat;
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
            routine.Functions[0].NewFakeInputParameter(11, 55);
            routine.Functions[0].NewFakeInputParameter(22, 155);
            routine.Functions[0].NewFakeInputParameter(33, 545);

            routine.Functions[1].NewFakeInputParameter(11, 55);
            routine.Functions[1].NewFakeInputParameter(22, 155);
            routine.Functions[1].NewFakeInputParameter(33, 545);

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

            bool TryAgain;
            do
            {
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
                if (Success)
                    TryAgain = false;
                else
                {
                    Console.WriteLine("Code generation without FILLED and NOT_INITIALIZED collision failed {0} times.", Common.MaxNumberOfRuns);
                    Console.WriteLine("Do you want to try again? (y/n)");
                    char answer = Convert.ToChar(Console.Read());
                    switch (answer)
                    {
                        case 'y':
                        case 'Y':
                            Console.WriteLine("Trying again...");
                            Console.WriteLine();
                            TryAgain = true;
                            break;

                        case 'n':
                        case 'N':
                            Console.WriteLine("Using the code with collision...");
                            Console.WriteLine();
                            TryAgain = false;
                            break;

                        default:
                            throw new Exception("Invalid answer!");
                    }
                }
            } while (TryAgain);

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

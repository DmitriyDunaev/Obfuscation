using Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Services;


namespace Obfuscator
{
    public static class ILObfuscator
    {
        private static void Obfuscation(Routine routine)
        {
            Logging.WriteComplexityMetrics(routine, "Original");
            Logging.WriteReadableTAC(routine, "Original1");
            Logging.WriteRoutine(routine, "Original1");

            //Checking whether the functions have either "division" or "modulo" operations
            //If they do, fake instructions with original variables should be inserted only before the return
            //to avoid problems with the these operations
            foreach (Function func in routine.Functions)
            {
                func.CheckDivisionModulo();
            }
            
            //Creating fake input parameters in all functions
            FakeParameters.CreateFakeParameters(routine);
            Logging.WriteReadableTAC(routine, "FakeParam2");

            //Checking for Multiple Obfuscation
            for (int i = 0; i < Convert.ToInt32(ConfigurationManager.AppSettings["MultipleRuns"]); i++)
            {
                //Creating fake variables
                foreach (Function func in routine.Functions)
                {
                    for (int j = 0; j < (Common.PercentageFakeVars * func.LocalVariables.FindAll(x => !x.fake).Count)/100; j++)
                    {
                        func.LocalVariables.Add(new Variable(Variable.Kind.Local, Variable.Purpose.Fake, Objects.Common.MemoryRegionSize.Integer));
                    }
                }

                Logging.WriteReadableTAC(routine, "FakeVariable3");

                if (ConfigurationManager.AppSettings["ConstCoverAlgInMultipleRuns"].Split('-')[i].Equals("1"))
                {
                    Console.Write("Step 1: Constants coverage");
                    ConstCoverage.CoverConstants(routine);
                    routine.Validate();
                    PrintSuccess();
                    Logging.WriteReadableTAC(routine, "CONST4");
                    Logging.DrawCFG(routine, "CONST");
                }

                if (ConfigurationManager.AppSettings["UncMeshingAlgInMultipleRuns"].Split('-')[i].Equals("1"))
                {
                    Console.Write("Step 2: Meshing unconditional jumps");
                    Meshing.MeshUnconditionals(routine);
                    routine.Validate();
                    PrintSuccess();
                    Logging.WriteReadableTAC(routine, "MeshingUNC5");
                    Logging.DrawCFG(routine, "MeshingUNC");
                }

                if (ConfigurationManager.AppSettings["CondMeshingAlgInMultipleRuns"].Split('-')[i].Equals("1"))
                {
                    Console.Write("Step 3: Meshing conditional jumps");
                    Meshing.MeshConditionals(routine);
                    routine.Validate();
                    PrintSuccess();
                    Logging.WriteReadableTAC(routine, "MeshingCOND6");
                    Logging.DrawCFG(routine, "MeshingCOND");
                }
                
                Console.Write("Step 4: Generation of fake NOP instructions");
                foreach (Function func in routine.Functions)
                    FakeCode.GenerateNoOperations(func);
                routine.Validate();
                PrintSuccess();
                Logging.WriteRoutine(routine, "NoOpersGeneration");
                Logging.WriteReadableTAC(routine, "FakeNOPs7");

                Console.Write("Step 5: Partial data analysis");
                DataAnalysis.GatherBasicBlockInfo(routine);
                PrintSuccess();

                if (ConfigurationManager.AppSettings["FakeJumpsAlgInMultipleRuns"].Split('-')[i].Equals("1"))
                {
                    Console.Write("Step 6: Generation of fake conditional jumps from NOPs");
                    foreach (Function func in routine.Functions)
                        FakeCode.GenerateConditionalJumps(func);
                    Logging.WriteRoutine(routine, "CondJumps");
                    foreach (Function func in routine.Functions)
                        FakeCode.GenerateNoOperations(func);
                    routine.Validate();
                    PrintSuccess();
                }

                Console.Write("Step 7: Complete data analysis");
                foreach (Function func in routine.Functions)
                    DataAnalysis.DeadVarsAlgortihm(func);
                DataAnalysis.GatherBasicBlockInfo(routine);
                PrintSuccess();


                Console.Write("Step 8: Generation of fake instructions from NOPs");
                foreach (Function func in routine.Functions)
                    FakeCode.GenerateFakeInstructions(func);
                Logging.WriteRoutine(routine, "FakeIns");
                Logging.DrawCFG(routine, "CondJumps");
                Logging.WriteComplexityMetrics(routine, "Final");
                FakeCode.CheckForProblems(routine);
                routine.Validate();
                PrintSuccess();

                Logging.WriteReadableTAC(routine, "FakeInstrFromNOPs8");
                Logging.WriteRoutine(routine, "FakeInstrFromNOPs8");
                Logging.WriteComplexityMetricsExcel(routine, "FakeInstrFromNOPs8 Excel");

                if (ConfigurationManager.AppSettings["OutliningAlgInMultipleRuns"].Split('-')[i].Equals("1"))
                {
                    Console.Write("Step 9: Generation new functions from Basic Blocks");
                    Outlining.generateFunctions(routine);
                    routine.Validate();
                    PrintSuccess();
                    Logging.WriteReadableTAC(routine, "BB9");
                    Logging.WriteRoutine(routine, "BB9");
                    Logging.DrawCFG(routine, "BB9");
                    Logging.WriteComplexityMetricsExcel(routine, "After BB Excel");
                }
            }
        }
        

        public static void Obfuscate(ref ExchangeFormat.Exchange exch)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\n\tINTERMEDIATE LEVEL OBFUSCATION\n");
            Console.ResetColor();

            Routine routine = (Routine)exch;

            Console.Write("Performing logical control");
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
                        Console.WriteLine(" . . . . . UNSUCCESSFUL\n");
                        Console.ResetColor();
                    }
                    NumberOfRuns++;
                } while (!Success && NumberOfRuns < Common.MaxNumberOfRuns);
                if (Success)
                    TryAgain = false;
                else
                {
                    //Console.WriteLine("Obfuscation failed {0} times. Possible problems: (1) FILLED and NOT_INITIALIZED collision; (2) basic blocks with single GOTO predecessor. Consequence: weaker result.", Common.MaxNumberOfRuns);
                    Console.WriteLine("Obfuscation process was unsuccessful {0} times. Consequence: weaker resilience.", Common.MaxNumberOfRuns);
                    Console.WriteLine("Do you want to try again? (y/n)");
                    char answer = Convert.ToChar(Console.Read());
                    switch (answer)
                    {
                        case 'y':
                        case 'Y':
                            Console.WriteLine("Trying again...");
                            Console.WriteLine();
                            TryAgain = true;
                            routine = (Routine)exch;
                            break;

                        case 'n':
                        case 'N':
                            Console.WriteLine("Using the resulting code...");
                            Console.WriteLine();
                            TryAgain = false;
                            break;

                        default:
                            throw new Exception("Invalid answer!");
                    }
                }
            } while (TryAgain);

            Console.Write("Logging TAC after obfuscation");
            Logging.WriteReadableTAC(routine);
            routine.Validate();
            PrintSuccess();

            Console.Write("Collecting and writing statistics");
            Logging.WriteStatistics(routine);
            PrintSuccess();

            Console.Write("Performing logical control");
            routine.Validate();
            PrintSuccess();

            exch = (ExchangeFormat.Exchange)routine;
        }

        
        public static void PrintSuccess()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            if (Console.CursorLeft % 2 != 0)
                Console.Write(" ");
            for (int i = Console.CursorLeft; i < 60; i += 2)
                Console.Write(". ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("SUCCESS\n");
            Console.ResetColor();
        }
    }
}

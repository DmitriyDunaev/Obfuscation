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
            Logging.WriteComplexityMetrics(routine, "Original");
            
            //Creating fake input parameters in all functions
            for (int i = 0; i < Common.NumFakeInputParam; i++)
            {
                List<int> borders = Randomizer.MultipleNumbers(2, Common.GlobalMinValue + Common.LoopConditionalJumpMaxRange,
                        Common.GlobalMaxValue - Common.LoopConditionalJumpMaxRange, false, true);
                foreach (Function func in routine.Functions)
                    func.NewFakeInputParameter(borders.First(), borders.Last());
            }

            //Updating all CALLs according to the new number of parameters
            foreach (Function func in routine.Functions)
            {
                func.UpdateAllCalls();
            }

            //Checking whether the functions have either "division" or "modulo" operations
            //If they do, fake instructions with original variables should be insert only right before the return
            //to avoid problems with the these instructions
            foreach (Function func in routine.Functions)
            {
                func.CheckDivisionModulo();
            }

            //Checking for Multiple Obfuscation
            for (int i = 0; i < Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["MultipleRuns"]); i++)
            {
                //Creating fake variables
                foreach (Function func in routine.Functions)
                {
                    for (int j = 0; j < (Common.PercentageFakeVars * func.LocalVariables.FindAll(x => !x.fake).Count)/100; j++)
                    {
                        func.LocalVariables.Add(new Variable(Variable.Kind.Local,Variable.Purpose.Fake,Common.MemoryRegionSize.Integer));
                    }
                }
                
                if (System.Configuration.ConfigurationSettings.AppSettings["ConstCoverAlgInMultipleRuns"].Split('-')[i].Equals("1"))
                {
                    Console.Write("Constants covering algorithm");
                    ConstCoverage.CoverConstants(routine);
                    routine.Validate();
                    PrintSuccess();
                    Logging.WriteReadableTAC(routine, "CONST");
                    Logging.DrawCFG(routine, "CONST");
                }

                if (System.Configuration.ConfigurationSettings.AppSettings["UncMeshingAlgInMultipleRuns"].Split('-')[i].Equals("1"))
                {
                    Console.Write("Meshing algorithm: Unconditional Jumps");
                    Meshing.MeshUnconditionals(routine);
                    routine.Validate();
                    PrintSuccess();
                    Logging.WriteReadableTAC(routine, "MeshingUNC");
                    Logging.DrawCFG(routine, "MeshingUNC");
                }
                
                if (System.Configuration.ConfigurationSettings.AppSettings["CondMeshingAlgInMultipleRuns"].Split('-')[i].Equals("1"))
                {
                    Console.Write("Meshing algorithm: Conditional Jumps");
                    Meshing.MeshConditionals(routine);
                    routine.Validate();
                    PrintSuccess();
                    Logging.WriteReadableTAC(routine, "MeshingCOND");
                    Logging.DrawCFG(routine, "MeshingCOND");
                }

                Console.Write("Generation of fake NOP instructions");
                foreach (Function func in routine.Functions)
                    FakeCode.GenerateNoOperations(func);
                routine.Validate();
                PrintSuccess();
                Logging.WriteRoutine(routine, "NoOpersGeneration");
                Logging.WriteReadableTAC(routine, "FakeNOPs");

                Console.Write("Prior data analysis");
                DataAnalysis.GatherBasicBlockInfo(routine);
                PrintSuccess();

                if (System.Configuration.ConfigurationSettings.AppSettings["FakeJumpsAlgInMultipleRuns"].Split('-')[i].Equals("1"))
                {
                    Console.Write("Generation of fake conditional jumps from NOPs");
                    foreach (Function func in routine.Functions)
                        FakeCode.GenerateConditionalJumps(func);
                    Logging.WriteRoutine(routine, "CondJumps");
                    foreach (Function func in routine.Functions)
                        FakeCode.GenerateNoOperations(func);
                    routine.Validate();
                    PrintSuccess();
                }

                Console.Write("Complete data analysis");
                foreach (Function func in routine.Functions)
                    DataAnalysis.DeadVarsAlgortihm(func);
                DataAnalysis.GatherBasicBlockInfo(routine);
                PrintSuccess();


                Console.Write("Generation of fake instructions from NOPs");
                foreach (Function func in routine.Functions)
                    FakeCode.GenerateFakeInstructions(func);
                Logging.WriteRoutine(routine, "FakeIns");
                Logging.DrawCFG(routine, "CondJumps");
                Logging.WriteComplexityMetrics(routine, "Final");
                routine.Validate();
                PrintSuccess();

                Logging.WriteReadableTAC(routine, "FakeInstrFromNOPs");
            }
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
                    //catch (ValidatorException ex)
                    //{
                    //    if (ex.Message.EndsWith("has a single direct predecessor with unconditional GOTO."))
                    //    {
                    //        Success = false;
                    //        Console.ForegroundColor = ConsoleColor.Red;
                    //        Console.WriteLine(". . . . . . . . . . FAILED\n");
                    //        Console.ResetColor();
                    //        routine = (Routine)exch;
                    //    }
                    //    else throw ex;
                    //}
                    NumberOfRuns++;
                } while (!Success && NumberOfRuns < Common.MaxNumberOfRuns);
                if (Success)
                    TryAgain = false;
                else
                {
                    //Console.WriteLine("Obfuscation failed {0} times. Possible problems: (1) FILLED and NOT_INITIALIZED collision; (2) basic blocks with single GOTO predecessor. Consequence: weaker result.", Common.MaxNumberOfRuns);
                    Console.WriteLine("Obfuscation failed {0} times. Possible problem: FILLED and NOT_INITIALIZED collision. Consequence: weaker result.", Common.MaxNumberOfRuns);
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

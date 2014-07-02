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
            //Creating fake input parameters
            int funcIndex = 0, i = 0;
            //In case the number of fake parameters is greater or equals to the number of functions, all fake parameters will be
            //created here
            while(i < Common.NumFakeInputParam)
            {
                //In case we have more fake parameters than functions, we need to check whether we have reached the last function
                //If we did then we start from the begining giving one more parameter until we reach the required number of fake
                //parameters
                if (funcIndex < routine.Functions.Count)
                {
                    List<int> borders = Randomizer.MultipleNumbers(2, Common.GlobalMinValue + Common.LoopConditionalJumpMaxRange, 
                        Common.GlobalMaxValue - Common.LoopConditionalJumpMaxRange, false, true);
                    routine.Functions[funcIndex].NewFakeInputParameter(borders.First(), borders.Last());
                    funcIndex++;
                    i++;
                }
                else
                    funcIndex = 0;
            }
            //In case we have less fake parameters than functions, we make a copy of the last one to all remaining functions
            if (Common.NumFakeInputParam < routine.Functions.Count)
            {
                //Index of the last function with fake parameter will be equals to the number of fake input parameters
                funcIndex = Common.NumFakeInputParam; 
                while (funcIndex < routine.Functions.Count)
                {
                    //We fetch the last fake parameter created and uses the same interval for the new one
                    //We need to do that in order to be able to use only one fake input as valid parameter 
                    //for more than one function in the future
                    Variable prevFakeInput = routine.Functions[funcIndex - 1].LocalVariables.FindAll(x => x.kind == Variable.Kind.Input && x.fake == true).Last();
                    routine.Functions[funcIndex].NewFakeInputParameter(prevFakeInput.fixedMin, prevFakeInput.fixedMax);
                    funcIndex++;
                }
            }

            //Updating all CALLs according to the new number of parameters
            foreach (Function func in routine.Functions)
            {
                func.UpdateAllCalls();
            }

            Console.Write("Constants covering algorithm");
            ConstCoverage.CoverConstants(routine);
            routine.Validate();
            PrintSuccess();

            Logging.WriteReadableTAC(routine, "CONST");
            Logging.DrawCFG(routine, "CONST");


            Console.Write("Meshing algorithm: Unconditional Jumps");
            Meshing.MeshUnconditionals(routine);
            routine.Validate();
            PrintSuccess();

            Logging.WriteReadableTAC(routine, "MeshingUNC");
            Logging.DrawCFG(routine, "MeshingUNC");
            

            Console.Write("Meshing algorithm: Conditional Jumps");
            Meshing.MeshConditionals(routine);
            routine.Validate();
            PrintSuccess();

            Logging.WriteReadableTAC(routine, "MeshingCOND");
            Logging.DrawCFG(routine, "MeshingCOND");
            

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
            /*foreach (Function func in routine.Functions)
                FakeCode.GenerateConditionalJumps(func);*/
            Logging.WriteRoutine(routine, "CondJumps");
            Logging.DrawCFG(routine, "CondJumps");
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

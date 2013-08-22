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
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\tINTERMEDIATE LEVEL OBFUSCATION\n");
            Console.ResetColor();

            Console.Write("Parsing XML data to Routine class");
            Routine routine = new Routine(exch);
            PrintSuccess();

            Console.Write("First validation of the routine");
            routine.Validate();
            PrintSuccess();

            Logging.WriteRoutine(routine, "Before");
            routine.Functions[1].NewFakeInputParameter(11, 55);
            routine.Functions[1].NewFakeInputParameter(22, 155);
            routine.Functions[1].NewFakeInputParameter(33, 545);
            Variable var = Randomizer.FakeInputParameter(routine.Functions[1]);
            Logging.WriteRoutine(routine, "After");

            Console.Write("Constants covering algorithm");
            ConstCoverage.CoverConstants(routine);
            routine.Validate();
            PrintSuccess();


            Console.Write("Meshing algorithm - Unconditional Jumps");
            Meshing.MeshUnconditionals(routine);
            routine.Validate();
            PrintSuccess();

            Console.Write("Meshing algorithm - Conditional Jumps");
            Meshing.MeshConditionals(routine);
            routine.Validate();
            PrintSuccess();

            //// TESTING PARSER
            //Variable left;
            //int right;
            //BasicBlock ts;
            //BasicBlock fs; 
            //Instruction.RelationalOperationType rel;
            //Parser.ConditionalJumpInstruction(routine.Functions[1].BasicBlocks[18].Instructions.Last(), out left, out right, out rel, out ts, out fs);
            //// END  TEST

            Console.Write("Generation of fake NoOperation instructions");
            foreach (Function func in routine.Functions)
                FakeCode.GenerateNoOperations(func);
            routine.Validate();
            PrintSuccess();

            Console.Write("Dead variables algorithm");
            foreach (Function func in routine.Functions)
                DataAnalysis.DeadVarsAlgortihm(func);
            routine.Validate();
            PrintSuccess();

            Console.Write("Generation of fake instructions from NoOperation");
            Console.WriteLine(". . . . COMING SOON");
            // Call algorithm here
            //routine.Validate();
            //PrintSuccess();
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

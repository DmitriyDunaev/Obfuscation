using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Objects;
using Services;

namespace Obfuscator
{
    public static class ConstCoverage
    {
        
        /// <summary>
        /// Covers constant values in the routine as a protective measure against a signature search 
        /// </summary>
        /// <param name="routine">A routine to be protected</param>
        public static void CoverConstants(Routine routine)
        {
            /*
             * We have to replace all the occurrences of the constant values,
             * so we should have a list of all the instructions using them.
             */
            foreach (Function func in routine.Functions)
            {
                foreach (BasicBlock bb in func.BasicBlocks)
                {
                    Dictionary<Instruction, int> constants = GetInstructionsWithConstants(bb);
                    foreach (Instruction ins in constants.Keys)
                        /* 
                         * Now that we have the list, we have to do the necessary
                         * insertions for all the instructions in the list.
                         */  
                        ProcessConstant(ins, constants[ins]);
                }
            }
        }

        /// <summary>
        /// Searches for instructions within a function, which contain a constant value
        /// </summary>
        /// <param name="basicblock">A basic block with instructions</param>
        /// <returns>A dictionary with the instruction and the contained constant value</returns>
        private static Dictionary<Instruction, int> GetInstructionsWithConstants(BasicBlock basicblock)
        {
            Dictionary<Instruction, int> consts = new Dictionary<Instruction, int>();
            foreach (Instruction inst in basicblock.Instructions)
            {
                if (inst.statementType == Objects.Common.StatementType.ConditionalJump)
                    continue;
                if(inst.statementType == Objects.Common.StatementType.Procedural && Regex.IsMatch(inst.TACtext, @"^call "))
                    continue;
                string resultString = null;
                resultString = Regex.Match(inst.TACtext, @"(^\d+)|(\s\d+)").Value.Trim();
                if (!string.IsNullOrEmpty(resultString))
                    consts.Add(inst, Convert.ToInt32(resultString));
            }
            return consts;
        }

        /// <summary>
        /// Inserts two additional instructions for constant coverage
        /// </summary>
        /// <param name="const_inst">The instruction with a constant value</param>
        /// <param name="const_value">Numerical constant value to be covered</param>
        private static void ProcessConstant(Instruction const_inst, int const_value)
        {
            List<int> two_places = Randomizer.MultipleNumbers(2, 0, const_inst.parent.Instructions.BinarySearch(const_inst), true, true);
            Instruction nop1 = new Instruction(const_inst.parent);
            Instruction nop2 = new Instruction(const_inst.parent);
            // We assume that we need 4 bytes for each constant.
            Variable t1 = const_inst.parent.parent.NewLocalVariable(Variable.Purpose.ConstRecalculation, Objects.Common.MemoryRegionSize.Integer);
            Variable t2 = const_inst.parent.parent.NewLocalVariable(Variable.Purpose.ConstRecalculation, Objects.Common.MemoryRegionSize.Integer);
            int first_number = Randomizer.SingleNumber(Common.GlobalMinValue, Common.GlobalMaxValue);
            Instruction.ArithmeticOperationType op = first_number < const_value ? Instruction.ArithmeticOperationType.Addition : Instruction.ArithmeticOperationType.Subtraction;
            int second_number = Math.Abs(first_number - const_value);
            MakeInstruction.Copy(nop1, t2, null, first_number);
            MakeInstruction.FullAssignment(nop2, t1, t2, null, second_number, op);
            const_inst.parent.Instructions.Insert(two_places[1], nop2);
            const_inst.parent.Instructions.Insert(two_places[0], nop1);
            const_inst.ModifyConstInstruction(t1, const_value);
        }
    }
}
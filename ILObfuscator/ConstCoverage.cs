#define PSEUDOCODE

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Internal;

namespace Obfuscator
{
    public static class ConstCoverage
    {
        
        public static void CoverConsts(Routine routine)
        {
            /*
             * We have to replace all the occurrences of the constant values,
             * so we should have a list of all the instructions using them.
             */
            foreach (Function func in routine.Functions)
            {
                foreach (BasicBlock bb in func.BasicBlocks)
                {
                    Dictionary<Instruction, int> tmp = GetInstructionsWithConstants(bb);
                    foreach (Instruction ins in tmp.Keys)
                        /* 
                         * Now that we have the list, we have to do the necessary
                         * insertions for all the instructions in the list.
                         */  
                        PreprocessConstant(ins, tmp[ins]);
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
                if (inst.statementType == ExchangeFormat.StatementTypeType.EnumValues.eConditionalJump)
                    continue;
                if(inst.statementType == ExchangeFormat.StatementTypeType.EnumValues.eProcedural && Regex.IsMatch(inst.TACtext, @"^call "))
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
        private static void PreprocessConstant(Instruction const_inst, int const_value)
        {
            List<int> two_places = Randomizer.GetMultipleNumbers(2, 0, const_inst.parent.Instructions.BinarySearch(const_inst), true, true);
            Instruction nop1 = new Instruction(ExchangeFormat.StatementTypeType.EnumValues.eNoOperation, const_inst.parent);
            Instruction nop2 = new Instruction(ExchangeFormat.StatementTypeType.EnumValues.eNoOperation, const_inst.parent);
            // We assume that we need 4 bytes for each constant.
            Variable t1 = const_inst.parent.parent.NewLocalVariable(Variable.Purpose.ConstRecalculation);
            Variable t2 = const_inst.parent.parent.NewLocalVariable(Variable.Purpose.ConstRecalculation);
            int first_number = Randomizer.GetSingleNumber(1, 99);
            Instruction.ArithmeticOperationType op = first_number < const_value ? Instruction.ArithmeticOperationType.Addition : Instruction.ArithmeticOperationType.Subtraction;
            int second_number = Math.Abs(first_number - const_value);
            nop1.MakeCopy(t2, null, first_number);
            nop2.MakeFullAssignment(t1, t2, null, second_number, op);
            const_inst.parent.Instructions.Insert(two_places[1], nop2);
            const_inst.parent.Instructions.Insert(two_places[0], nop1);
            const_inst.ModifyConstInstruction(t1, const_value);
        }
    }
}

















/*
 * TODO:
 * 
 * - Routine: search_for_constants() - it searches for all the instructions containing
 *                                     constants (with that regexp thing), and returns
 *                                     a list of pairs that hold an instruction and a
 *                                     constant (integer) value.
 *                   
 * - Function: add_vars() - adds a given set of variables of the function's variable list
 *                   
 * - BasicBlock: insert_before_inst() - inserts a given set of instructions before the given
 *                                      instruction into the basic block's instruction list
 *                                      
 * - Instruction: find_and_replace() - finds the given constant number in the text string of
 *                                     the instruction, and replaces it with the name of the
 *                                     Variable parameter.
 *                                     also adds the variable to the RefVariables list.
 *                Instruction() - constructor, so that this algorithm can make instructions, too
 *                                      
 * - Variable: Variable() - constructor for making a temporary variable, with unique name
 *                          (like tmp_xxx, or something like that)
 */

#if !PSEUDOCODE

/*
 * Structure to hold the pairs of instructions and constants.
 */
struct ins_n_cons
{
    Instruction ins;
    int cons;
}

void recalc ()
{
    /*
     * We have to replace all the occurrences of the constant values,
     * so we should have a list of all the instructions using them.
     * 
     * search_for_constants() - gives a list of the instructions that are
     *                          using constants
     */
    List<ins_and_cons> inst = search_for_constants();

    foreach (Instruction i in inst)
    {
        /*
         * modify_ins() - it replaces the constant with a temporary variable
         *                and returns this temporary variable
         */
        Variable tmp = modify_ins(i);

        /*
         * calc_const() - from the given costant, and a temporary variable it
         *                generates a set of instructions which calculate the
         *                constant dynamically, and assigns it to the temporary variable.
         *                it gives us a list of the altered instructions,
         *                and a list of the new (temporary, local) variables used.
         */
        list<Instructions> newins;
        list<Variables> newvars;
        calc_const (i.cons, tmp, newvars, newins);

        /*
         * We have to insert the new set of instructions before the place of
         * the original instruction, and we have to insert the new variables
         * to the variable list of the function.
         * 
         * Instruction::parent - the basic block that contains the given instruction
         * 
         * insert_before_inst() - inserts a given set of instructions before the given
         *                        instruction into the basic block's instruction list
         *                        
         * BasicBlock::parent - returns the function that contains the given basic block
         * 
         * add_vars() - inserts a given set of variables to the function's variable list
         */
         i.ins.parent.insert_before_inst (i.ins, newins);
         i.ins.parent.parent.add_vars (newvars);
    }
}

Variable modify_ins (inst_and_const i)
{
    /*
     * We generate a temporary variable to hold the value of the constant,
     * and we put it in the newvars list.
     * After that we find the constant in the instruction, replace it with the
     * brand new temporary variable, and set the referenced variables of the instruction.
     * 
     * new Variable() - it should return a new variable with a unique name
     *                  like tmp_xxx (or something like that), where xxx comes
     *                  from a static variable of the Variable class, which 
     *                  increments every time this constructor runs
     *                      
     * find_and_replace() - find the occurrence of the given number in the string,
     *                      and replaces it with the name of the new temporary variable.
     *                      it puts the temporary variable in the referenced variables list
     *                      (i am not sure if it is necessary, though. unless we want to
     *                      use these temporary variables for further obfuscation...)
     */
    Variable tmp = new Variable(_temporary_);
    i.ins.find_and_replace (i.cons, tmp);
    return tmp;
}

/*
 * Here comes the algorithm that dinamically counts the constant value.
 * 
 * Now its just a prototype, but everything is given to make it much better,
 * because no matter how complex the algorithm is, it only makes some instructions,
 * and some temporary variables, and it can.
 */
void calc_const (int cons, Variable tmp, List<Variable> newvars, List<Instructions> newins)
{
    /*
     * Prototype algortihm: const -> tmp | tmp := (const + 10) - 10;
     * I know it is not too smart, but hopefully it works and by now, it is enough :)
     * 
     * For the instruction we make a refvars list (wich contains the only one tmp variable),
     * and we generate the instruction's text.
     * 
     * Instruction() - constructor usable by the algorithm
     */
    List<Variables> refvars;
    refvars.add(tmp);
    string text;
    text << tmp.getname() << " := " << cons + 10 << " - " << 10;
    newins.add ( new Instruction(fake, refvars, text, ...) );
}

#endif

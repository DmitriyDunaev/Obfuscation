using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ExchangeFormat;
using System.Collections.Generic;

namespace Objects
{
    public interface IValidate
    {
        void Validate();
    }


    public partial class Routine
    {
        public void Validate()
        {
            foreach (Variable var in this.GlobalVariables)
            {
                if (var.kind != Variable.Kind.Global)
                    throw new ValidatorException("Global variable's 'Kind' property must be set to 'Global'. Variable: " + var.ID);
                if (string.IsNullOrWhiteSpace(var.globalID))
                    throw new ValidatorException("Global variable must have non-empty GlobalID property. Variable: " + var.ID);
                var.Validate();
            }
            foreach (Function func in this.Functions)
            {
                if (func.parent != this)
                    throw new ValidatorException("Function's 'parent' property is incorrect. Function: " + func.ID);
                func.Validate();
            }
        }
    }


    public partial class Function
    {
        public void Validate()
        {
            if (this.calledFrom == Common.CalledFrom.ExternalOnly || this.calledFrom == Common.CalledFrom.Both)
                if (string.IsNullOrWhiteSpace(globalID))
                    throw new ValidatorException("The function, which can be called from outside the routine, must have a non-empty GlobalID property. Function: " + this.ID);

            if (BasicBlocks.FindAll(x => x.getSuccessors.Count == 0 && x.getPredecessors.Count > 0 && x.Instructions.Count == 1 && x.Instructions[0].TACtext == "return").Count == 0)
                throw new ValidatorException("Function " + ID + " has no 'fake exit block'.");

            if (BasicBlocks.FindAll(x => x.getSuccessors.Count == 0 && x.getPredecessors.Count > 0 && x.Instructions.Count == 1 && x.Instructions[0].TACtext == "return").Count > 1)
                throw new ValidatorException("Function " + ID + " has more than one 'fake exit block'.");

            foreach (BasicBlock bb in this.BasicBlocks)
            {
                if (bb.parent != this)
                    throw new ValidatorException("Basic block's 'parent' property is incorrect. Basic block: " + bb.ID);
                bb.Validate();
            }

            /* Instructions are unique, so they can be referenced only once within a function */
            List<Instruction> all_instructions = new List<Instruction>();
            BasicBlocks.ForEach(x => all_instructions.AddRange(x.Instructions));
            if (all_instructions.Count != all_instructions.Distinct().Count())
                throw new ValidatorException("Multiple references to the same instruction.");
        }
    }


    public partial class BasicBlock
    {
        public void Validate()
        {
            if (RefSuccessors.Count > 0)
                throw new ValidatorException("Basic block contains a reference to successors that could not be resolved. Basic block: " + ID);
            if (RefPredecessors.Count > 0)
                throw new ValidatorException("Basic block contains a reference to successors that could not be resolved. Basic block: " + ID);

            if (Instructions.Last().statementType == Common.StatementType.ConditionalJump && Successors.Count != 2)
                throw new ValidatorException("Basic block with ConditionalJump statement type must have exactly 2 successors.");
            if (Predecessors.Count == 0 && Successors.Count == 0)
                throw new ValidatorException("No predecessors and no successors found for basic block " + ID);
            if (Instructions.Count == 0)
                throw new ValidatorException("No instructions found for basic block " + ID);
            //if (Successors.Count > 2) //TODO NOT A PROBLEM ANYMORE
            //    throw new ValidatorException("More than two successors found in basic block " + ID);
            if (Predecessors.Count == 0 && !parent.BasicBlocks.First().Equals(this))
                throw new ValidatorException("No predecessors found at basic block " + ID + ". By convention, only the first basic block has empty Predecessors list.");
            if (Successors.Count == 0 && (Instructions.Count != 1 || Instructions[0].TACtext != "return"))
                throw new ValidatorException("No successors found at basic block " + ID + ". By convention, only the 'fake exit block' has empty Successors list.");
           /* if (Successors.Count == 2) //TODO NOT A PROBLEM ANYMORE
            {
                if (Instructions.Last().statementType != Common.StatementType.ConditionalJump)
                    throw new ValidatorException("To have 2 successors, the last instruction of a basic block must be a ConditionalJump. Basic block: " + ID);
                string resultString = Regex.Match(Instructions.Last().TACtext, @"\bID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b").Value;
                if (!resultString.Equals(Successors[0].ID))
                    throw new ValidatorException("The first successor ID does not match the GOTO instruction. Basic block: " + ID);
            }*/
            if (Predecessors.Count > 1 && Successors.Count > 0)
            {
                int preds_goto = Predecessors.Count(x =>
                    (x.Instructions.Last().statementType == Common.StatementType.ConditionalJump && this.Equals(x.getSuccessors.First()))
                    || x.Instructions.Last().statementType == Common.StatementType.UnconditionalJump);
                if (Predecessors.Count - preds_goto > 1)
                    throw new ValidatorException("Basic block has more that one direct predecessor (without GOTO). Basic block: " + ID);
            }
            if (Successors.Count == 0)
            {
                int preds_proc = Predecessors.Count(x =>
                    x.Instructions.Last().statementType == Common.StatementType.Procedural);
                if (Predecessors.Count - preds_proc > 0)
                    throw new ValidatorException("The 'fake exit block' can only have predecessors with last 'return' statement. Basic block: " + ID);
            }

            // If a basic block has one predecessor, then it should be a direct predecessor without goto
            //if(Predecessors.Count==1 && Predecessors[0].Instructions.Last().statementType == Common.StatementType.UnconditionalJump)
            //    throw new ValidatorException("A basic block: " + ID + " has a single direct predecessor with unconditional GOTO.");

            // Checking Successor-Predecessor links
            foreach (BasicBlock succ in Successors)
                if (!succ.Predecessors.Contains(this) && succ.RefPredecessors.Count == 0)
                    throw new ValidatorException("Broken successor-predecessor link in basic block " + ID);
            foreach (BasicBlock pred in Predecessors)
                if (!pred.Successors.Contains(this) && pred.RefSuccessors.Count == 0)
                    throw new ValidatorException("Broken predecessor-successor link in basic block " + ID);

            foreach (Instruction inst in this.Instructions)
            {
                if (inst.parent != this)
                    throw new ValidatorException("Instruction's 'parent' property is incorrect. Instruction: " + inst.ID);
                inst.Validate();
            }
        }
    }


    public partial class Instruction
    {
        public void Validate()
        {
            //Test 1: Depending on Statement type the number of RefVars is fixed
            switch (statementType)
            {
                case Common.StatementType.FullAssignment:
                    if (RefVariables.Count < 1 || RefVariables.Count > 3)
                        throw new ValidatorException("Number of referenced variables does not match statement type in instruction " + ID);
                    break;
                case Common.StatementType.UnaryAssignment:
                case Common.StatementType.PointerAssignment:
                case Common.StatementType.ConditionalJump:
                    if (RefVariables.Count < 1 || RefVariables.Count > 2)
                        throw new ValidatorException("Number of referenced variables does not match statement type in instruction " + ID);
                    break;
                case Common.StatementType.Copy:
                    if (RefVariables.Count < 1 || RefVariables.Count > 2)
                        throw new ValidatorException("Number of referenced variables does not match statement type in instruction " + ID);
                    if (RefVariables.First().pointer != RefVariables.Last().pointer)
                        throw new ValidatorException("You are trying to copy a pointer value to a non-pointer variable (or vice-versa) in COPY instruction " + ID);
                    break;
                case Common.StatementType.UnconditionalJump:
                case Common.StatementType.NoOperation:
                    if (RefVariables.Count != 0)
                        throw new ValidatorException("Number of referenced variables does not match statement type in instruction " + ID);
                    break;
                case Common.StatementType.Procedural:
                    if (RefVariables.Count > 1)
                        throw new ValidatorException("Number of referenced variables does not match statement type in instruction " + ID);
                    break;
                case Common.StatementType.IndexedAssignment:
                    if (RefVariables.Count < 2 || RefVariables.Count > 3)
                        throw new ValidatorException("Number of referenced variables does not match statement type in instruction " + ID);
                    break;
                default:
                    throw new ValidatorException("Invalid statement type in instruction " + ID);
            }

            // Test 2: TAC text by Regexp
            switch (statementType)
            {
                case Common.StatementType.FullAssignment:
                    // var := var op var OR var:= var op number
                    if (Regex.IsMatch(TACtext, @"^[vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} := [vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} [-+*/%] ([vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12})$", RegexOptions.None)
                        | Regex.IsMatch(TACtext, @"^[vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} := [vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} [-+*/%] ([-+]?\d+)$", RegexOptions.None))
                        break;
                    else
                        throw new ValidatorException("The instruction Text value does not match its StatementType property. Instruction: " + ID);
                case Common.StatementType.UnaryAssignment:
                    // var := op var
                    if (Regex.IsMatch(TACtext, @"^[vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} := [-!] [vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None))
                        break;
                    else
                        throw new ValidatorException("The instruction Text value does not match its StatementType property. Instruction: " + ID);
                case Common.StatementType.Copy:
                    // var := var OR var := number
                    if (Regex.IsMatch(TACtext, @"^[vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} := [vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) |
                        Regex.IsMatch(TACtext, @"^[vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} := ([-+]?\d+)$", RegexOptions.None))
                        break;
                    else
                        throw new ValidatorException("The instruction Text value does not match its StatementType property. Instruction: " + ID);
                case Common.StatementType.UnconditionalJump:
                    // goto ID_GUID
                    if (Regex.IsMatch(TACtext, @"^goto ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None))
                        break;
                    else
                        throw new ValidatorException("The instruction Text value does not match its StatementType property. Instruction: " + ID);
                case Common.StatementType.ConditionalJump:
                    // if var relop var goto ID_GUID OR if var relop number goto ID_GUID
                    if (Regex.IsMatch(TACtext, @"^if [vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} (?:==|!=|>|<|>=|<=) [vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} goto ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) |
                        Regex.IsMatch(TACtext, @"^if [vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} (?:==|!=|>|<|>=|<=) ([-+]?\d+) goto ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None))
                        break;
                    else
                        throw new ValidatorException("The instruction Text value does not match its StatementType property. Instruction: " + ID);
                case Common.StatementType.Procedural:
                    // param var OR param number
                    if (Regex.IsMatch(TACtext, @"^param [vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) |
                        Regex.IsMatch(TACtext, @"^param ([-+]?\d+)$", RegexOptions.None) |
                        // call some_string decimal
                        Regex.IsMatch(TACtext, @"^call \w+ \d+$", RegexOptions.None) |
                        // call ID_GUID decimal
                        Regex.IsMatch(TACtext, @"^call ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} \d+$", RegexOptions.None) |
                        // return 
                        Regex.IsMatch(TACtext, @"^return$", RegexOptions.None) |
                        // return number
                        Regex.IsMatch(TACtext, @"^return ([-+]?\d+)$", RegexOptions.None) |
                        // return var
                        Regex.IsMatch(TACtext, @"^return [vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) |
                        // retrieve var
                        Regex.IsMatch(TACtext, @"^retrieve [vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) |
                        // enter ID_GUID
                        Regex.IsMatch(TACtext, @"^enter ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) |
                        // leave ID_GUID
                        Regex.IsMatch(TACtext, @"^leave ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None)
                        )
                        break;
                    else
                        throw new ValidatorException("The instruction Text value does not match its StatementType property. Instruction: " + ID);
                case Common.StatementType.IndexedAssignment:
                    // var := var[number] OR var[number] := var OR var[number] = number;
                    //if (Regex.IsMatch(TACtext, @"^[vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} := [vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} \[\d+\]$", RegexOptions.None) |
                    //    Regex.IsMatch(TACtext, @"^[vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} \[\d+\] := [vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) |
                    //    Regex.IsMatch(TACtext, @"^[vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} \[\d+\] := ([-+]?\d+)$", RegexOptions.None))
                    //    break;
                    //else
                    //    throw new ValidatorException("The instruction Text value does not match its StatementType property. Instruction: " + ID);
                    throw new ValidatorException("The 'IndexedAssignment' statement type is not supported. Please use 'PointerAssignment' instead. Instruction: " + ID);
                case Common.StatementType.PointerAssignment:
                    // var := & var
                    if (Regex.IsMatch(TACtext, @"^[vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} := & [vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) |
                        // var := * var
                        Regex.IsMatch(TACtext, @"^[vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} := \* [vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) |
                        // * var := var 
                        Regex.IsMatch(TACtext, @"^\* [vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} := [vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) |
                        // * var = number
                        Regex.IsMatch(TACtext, @"^\* [vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} := ([-+]?\d+)$", RegexOptions.None))
                        break;
                    else
                        throw new ValidatorException("The instruction Text value does not match its StatementType property. Instruction: " + ID);
                case Common.StatementType.NoOperation:
                    if (Regex.IsMatch(TACtext, @"^nop$", RegexOptions.None))
                        break;
                    else
                        throw new ValidatorException("The instruction Text value does not match its StatementType property. Instruction: " + ID);
                default:
                    break;
            }

            // Test 3: RETRIEVE instruction must be preceeded by CALL
            int num = parent.Instructions.BinarySearch(this);
            if (Regex.IsMatch(TACtext, @"^retrieve [vtcfp]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None))
            {
                if (num == 0)
                    throw new ValidatorException("The first instruction in a basic block cannot be 'retrieve'. Instruction: " + ID);
                else
                    if (!(
                        Regex.IsMatch(parent.Instructions[num - 1].TACtext, @"^call \w+ \d+$", RegexOptions.None) ||
                        Regex.IsMatch(parent.Instructions[num - 1].TACtext, @"^call ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} \d+$", RegexOptions.None)
                        ))
                        throw new ValidatorException("Instruction 'retrieve' must be directly preceeded by instruction 'call'. Instruction: " + ID);
            }
        }
    }


    public partial class Variable
    {
        public void Validate()
        {
            if (!pointer && (memoryUnitSize != memoryRegionSize))
                throw new ValidatorException("Non-pointer variable cannot have MemoryUnitSize not equal to MemoryRegionSize. Variable: " + ID);
            if (fixedMin.HasValue && fixedMax.HasValue && (fixedMin.Value > fixedMax.Value))
                throw new ValidatorException("MinValue must be smaller than MaxValue for variable " + ID);
            if (fake && kind == Kind.Input && !(fixedMin.HasValue || fixedMax.HasValue))
                throw new ValidatorException("Fake input variable must have at least one of MinValue or MaxValue attributes. Variable " + ID);
        }
    }
       
}

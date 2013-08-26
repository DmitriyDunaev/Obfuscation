using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Obfuscator;
using ExchangeFormat;
using System.Collections.Generic;

namespace Internal
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
            if (this.calledFrom == CalledFromType.EnumValues.eExternalOnly || this.calledFrom == CalledFromType.EnumValues.eBoth)
                if (string.IsNullOrWhiteSpace(globalID))
                    throw new ValidatorException("The function, which can be called from outside the routine, must have a non-empty GlobalID property. Function: " + this.ID);

            //if (!Regex.IsMatch(BasicBlocks[0].Instructions[0].text, @"^enter ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) ||
            //    !Regex.IsMatch(BasicBlocks[BasicBlocks.Count-1].Instructions[0].text, @"^leave ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None))
            //    throw new ValidatorException("No 'enter' and/or 'leave' instructions found in function " + ID);

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

            /* We shouldn't have the same Instructions referenced multiple times. */
            List<Instruction> inslist = new List<Instruction>();
            HashSet<Instruction> inshash = new HashSet<Instruction>();
            foreach (BasicBlock bb in BasicBlocks)
            {
                foreach (Instruction ins in bb.Instructions)
                {
                    inslist.Add(ins);
                    inshash.Add(ins);
                }
            }
            if (inslist.Count != inshash.Count)
                throw new ValidatorException("Multiple references to the same instruction.");
        }
    }


    public partial class BasicBlock
    {
        public void Validate()
        {
            // At first creation of BB
            if (RefSuccessors.Count() > 0 && Successors.Count() == 0)
            {
                foreach (string id in RefSuccessors)
                    foreach (BasicBlock bb in parent.BasicBlocks)
                        if (bb.ID == id)
                            Successors.Add(bb);
                if (!RefSuccessors.Count.Equals(Successors.Count))
                    throw new ValidatorException("Referenced basic block was not found. Referenced block:" + RefSuccessors[0]);
                RefSuccessors.Clear();
            }

            if (Instructions.Last().statementType == StatementTypeType.EnumValues.eConditionalJump && Successors.Count != 2)
                throw new ValidatorException("Basic block with ConditionalJump statement type must have exactly 2 successors.");
            if (RefPredecessors.Count() > 0 && Predecessors.Count() == 0)
            {
                foreach (string id in RefPredecessors)
                    foreach (BasicBlock bb in parent.BasicBlocks)
                        if (bb.ID == id)
                            Predecessors.Add(bb);
                if (!RefPredecessors.Count.Equals(Predecessors.Count))
                    throw new ValidatorException("Referenced basic block was not found. Referenced block:" + RefPredecessors[0]);
                RefPredecessors.Clear();
            }

            // Actual validation
            if (Predecessors.Count == 0 && Successors.Count == 0)
                throw new ValidatorException("No predecessors and no successors found for basic block " + ID);
            if (Instructions.Count == 0)
                throw new ValidatorException("No instructions found for basic block " + ID);
            if (Successors.Count > 2)
                throw new ValidatorException("More than two successors found in basic block " + ID);
            if (Predecessors.Count == 0 && !parent.BasicBlocks.First().Equals(this))
                throw new ValidatorException("No predecessors found at basic block " + ID + ". By convention, only the first basic block has empty Predecessors list.");
            if(Successors.Count == 0 && (Instructions.Count != 1 || Instructions[0].TACtext != "return"))
                throw new ValidatorException("No successors found at basic block " + ID + ". By convention, only the 'fake exit block' has empty Successors list.");
            if (Successors.Count == 2)
            {
                if (Instructions.Last().statementType != StatementTypeType.EnumValues.eConditionalJump)
                    throw new ValidatorException("To have 2 successors, the last instruction of a basic block must be a ConditionalJump. Basic block: " + ID);
                string resultString = null;
                //bool found = false;
                resultString = Regex.Match(Instructions.Last().TACtext, @"\bID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b").Value;
                //foreach (BasicBlock bb in Successors)
                //{
                //    if (resultString.Equals(bb.ID))
                //        found = true;
                //}
                //if (!found)
                if(!resultString.Equals(Successors[0].ID))
                    throw new ValidatorException("The first successor ID does not match the GOTO instruction. Basic block: " + ID);
                    //throw new ValidatorException("The successors' IDs do not match last GOTO instruction. Basic block: " + ID);
            }
            //if (Predecessors.Count == 1 && Predecessors[0].Instructions.Last().statementType == StatementTypeType.EnumValues.eUnconditionalJump)
            //{
            //    throw new ValidatorException("Basic block has one single predecessor with unconditional jump to it. The last GOTO instruction of predecessor can be deleted. Basic block: " + ID);
            //}

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
                case StatementTypeType.EnumValues.eFullAssignment:
                    if (RefVariables.Count < 1 || RefVariables.Count > 3)
                        throw new ValidatorException("Number of referenced variables does not match statement type in instruction " + ID);
                    break;
                case StatementTypeType.EnumValues.eUnaryAssignment:
                case StatementTypeType.EnumValues.ePointerAssignment:
                case StatementTypeType.EnumValues.eConditionalJump:
                    if (RefVariables.Count < 1 || RefVariables.Count > 2)
                        throw new ValidatorException("Number of referenced variables does not match statement type in instruction " + ID);
                    break;
                case StatementTypeType.EnumValues.eCopy:
                    if (RefVariables.Count < 1 || RefVariables.Count > 2)
                        throw new ValidatorException("Number of referenced variables does not match statement type in instruction " + ID);
                    if (RefVariables.First().pointer != RefVariables.Last().pointer)
                        throw new ValidatorException("You are trying to copy a pointer value to a non-pointer variable (or vice-versa) in COPY instruction " + ID);
                    break;
                case StatementTypeType.EnumValues.eUnconditionalJump:
                case StatementTypeType.EnumValues.eNoOperation:
                    if (RefVariables.Count != 0)
                        throw new ValidatorException("Number of referenced variables does not match statement type in instruction " + ID);
                    break;
                case StatementTypeType.EnumValues.eProcedural:
                    if (RefVariables.Count > 1)
                        throw new ValidatorException("Number of referenced variables does not match statement type in instruction " + ID);
                    break;
                case StatementTypeType.EnumValues.eIndexedAssignment:
                    if (RefVariables.Count < 2 || RefVariables.Count > 3)
                        throw new ValidatorException("Number of referenced variables does not match statement type in instruction " + ID);
                    break;
                case StatementTypeType.EnumValues.Invalid:
                default:
                    throw new ValidatorException("Invalid statement type in instruction " + ID);
            }

            // Test 2: TAC text by Regexp
            switch (statementType)
            {
                case StatementTypeType.EnumValues.eFullAssignment:
                    // var := var op var OR var:= var op number
                    if (Regex.IsMatch(TACtext, @"^[vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} := [vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} [-+*/] ([vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12})$", RegexOptions.None)
                        | Regex.IsMatch(TACtext, @"^[vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} := [vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} [-+*/] ([-+]?\d+)$", RegexOptions.None))
                        break;
                    else
                        throw new ValidatorException("The instruction Text value does not match its StatementType property. Instruction: " + ID);
                case StatementTypeType.EnumValues.eUnaryAssignment:
                    // var := op var
                    if (Regex.IsMatch(TACtext, @"^[vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} := [-!] [vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None))
                        break;
                    else
                        throw new ValidatorException("The instruction Text value does not match its StatementType property. Instruction: " + ID);
                case StatementTypeType.EnumValues.eCopy:
                    // var := var OR var := number
                    if (Regex.IsMatch(TACtext, @"^[vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} := [vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) |
                        Regex.IsMatch(TACtext, @"^[vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} := ([-+]?\d+)$", RegexOptions.None))
                        break;
                    else
                        throw new ValidatorException("The instruction Text value does not match its StatementType property. Instruction: " + ID);
                case StatementTypeType.EnumValues.eUnconditionalJump:
                    // goto ID_GUID
                    if (Regex.IsMatch(TACtext, @"^goto ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None))
                        break;
                    else
                        throw new ValidatorException("The instruction Text value does not match its StatementType property. Instruction: " + ID);
                case StatementTypeType.EnumValues.eConditionalJump:
                    // if var relop var goto ID_GUID OR if var relop number goto ID_GUID
                    if (Regex.IsMatch(TACtext, @"^if [vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} (?:==|!=|>|<|>=|<=) [vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} goto ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) |
                        Regex.IsMatch(TACtext, @"^if [vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} (?:==|!=|>|<|>=|<=) ([-+]?\d+) goto ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None))
                        break;
                    else
                        throw new ValidatorException("The instruction Text value does not match its StatementType property. Instruction: " + ID);
                case StatementTypeType.EnumValues.eProcedural:
                    // param var OR param number
                    if (Regex.IsMatch(TACtext, @"^param [vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) |
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
                        Regex.IsMatch(TACtext, @"^return [vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) |
                        // retrieve var
                        Regex.IsMatch(TACtext, @"^retrieve [vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) |
                        // enter ID_GUID
                        Regex.IsMatch(TACtext, @"^enter ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) |
                        // leave ID_GUID
                        Regex.IsMatch(TACtext, @"^leave ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None)
                        )
                        break;
                    else
                        throw new ValidatorException("The instruction Text value does not match its StatementType property. Instruction: " + ID);
                case StatementTypeType.EnumValues.eIndexedAssignment:
                    // var := var[number] OR var[number] := var OR var[number] = number;
                    //if (Regex.IsMatch(TACtext, @"^[vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} := [vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} \[\d+\]$", RegexOptions.None) |
                    //    Regex.IsMatch(TACtext, @"^[vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} \[\d+\] := [vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) |
                    //    Regex.IsMatch(TACtext, @"^[vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} \[\d+\] := ([-+]?\d+)$", RegexOptions.None))
                    //    break;
                    //else
                    //    throw new ValidatorException("The instruction Text value does not match its StatementType property. Instruction: " + ID);
                    throw new ValidatorException("The 'IndexedAssignment' statement type is not supported. Please use 'PointerAssignment' instead. Instruction: " + ID);
                case StatementTypeType.EnumValues.ePointerAssignment:
                    // var := & var
                    if (Regex.IsMatch(TACtext, @"^[vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} := & [vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) |
                        // var := * var
                        Regex.IsMatch(TACtext, @"^[vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} := \* [vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) |
                        // * var := var 
                        Regex.IsMatch(TACtext, @"^\* [vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} := [vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$", RegexOptions.None) |
                        // * var = number
                        Regex.IsMatch(TACtext, @"^\* [vtcfd]_ID_[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12} := ([-+]?\d+)$", RegexOptions.None))
                        break;
                    else
                        throw new ValidatorException("The instruction Text value does not match its StatementType property. Instruction: " + ID);
                case StatementTypeType.EnumValues.eNoOperation:
                    if (Regex.IsMatch(TACtext, @"^nop$", RegexOptions.None))
                        break;
                    else
                        throw new ValidatorException("The instruction Text value does not match its StatementType property. Instruction: " + ID);
                default:
                    break;
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

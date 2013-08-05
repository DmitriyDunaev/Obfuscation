using ExchangeFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Obfuscator
{
    public interface IValidate
    {
        void Validate();
    }

    public partial class Routine
    {
        public void Validate()
        {
            if(this.Functions.Count==0)
                throw new ValidatorException("Routine has no functions!");
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
            if (this.calledFrom == ExchangeFormat.CalledFromType.EnumValues.eExternalOnly || this.calledFrom == ExchangeFormat.CalledFromType.EnumValues.eBoth)
                if (string.IsNullOrWhiteSpace(globalID))
                    throw new ValidatorException("The function, which can be called from outside the routine, must have a non-empty GlobalID property. Function: " + this.ID.ToString());

            foreach (BasicBlock bb in this.BasicBlocks)
            {
                if(bb.parent != this)
                    throw new ValidatorException("Basic block's 'parent' property is incorrect. Basic block: " + bb.ID); 
            }
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
                throw new ValidatorException("No predecessors and no successors found for basic block " + ID.ToString());
            if (Instructions.Count == 0)
                throw new ValidatorException("No instructions found for basic block " + ID.ToString());
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
            switch (this.statementType)
            {
                case StatementTypeType.EnumValues.eFullAssignment:
                    if (RefVariables.Count < 1 || RefVariables.Count > 3)
                        throw new ValidatorException("Number of referenced variables does not match statement type in instruction " + ID);
                    break;
                case StatementTypeType.EnumValues.eUnaryAssignment:
                case StatementTypeType.EnumValues.eCopy:
                case StatementTypeType.EnumValues.ePointerAssignment:
                case StatementTypeType.EnumValues.eConditionalJump:
                    if (RefVariables.Count < 1 || RefVariables.Count > 2)
                        throw new ValidatorException("Number of referenced variables does not match statement type in instruction " + ID);
                    break;
                case StatementTypeType.EnumValues.eUnconditionalJump:
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
        }
    }

    public partial class Variable
    {
        public void Validate()
        {
        }
    }



    public static class Validator
    {
        public static void ValidateXML(XmlDocument doc2validate)
        {
            System.Xml.Schema.XmlSchemaSet schemas = new System.Xml.Schema.XmlSchemaSet();
            schemas.Add(null, @"Schemas\Exchange.xsd");
            try
            {
                XDocument doc = XDocument.Parse(doc2validate.InnerXml);
                doc.Validate(schemas, (o, e) =>
                {
                    throw new ValidatorException(e.Message);
                });
            }
            catch (Exception ex)
            {
                throw new ValidatorException("XML could not be validated! It is not well-formed or does not comply with XSD.", ex);
            }
        }
    }

    public class ValidatorException : Exception
    {
        public ValidatorException(string message)
            : base(message)
        { }

        public ValidatorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

}

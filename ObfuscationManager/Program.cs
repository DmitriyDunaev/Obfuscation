using System;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonModule;
using System.Xml;
using System.IO;
using Obfuscator;


namespace ObfuscationManager
{

    class Manager
    {


        protected static void Example()
        {
            //
            // TODO:
            //   Insert your code here...
            //
            // Example code to create and save a structure:
//            Obfuscator.Exchange doc = Obfuscator.Exchange.CreateDocument();
                        
//            Obfuscator.RoutineType routine = doc.Routine.Append();
//            routine.Description.Value = "Some description...";
//            doc.SetSchemaLocation(@"Schemes\Exchange.xsd");

//            Obfuscator.FunctionType func1 = routine.Function.Append();
//            func1.ID.Value = "ID_1";
//            func1.CalledFrom.EnumerationValue = Obfuscator.CalledFromType.EnumValues.eInternalOnly;
//            func1.ExternalLabel.Value = "ZZZ";

//            Obfuscator.VariablesType vars1 = func1.Inputs.Append();

//            Obfuscator.OriginalType orig_vars = vars1.Original.Append();
////            Obfuscator.FakeType fake_vars = vars1.Fake.Append();
            
//            Obfuscator.VariableType v1 = orig_vars.Variable.Append();
//            v1.ConstValueInParam.Value = "4";
//            v1.ID.Value = "ID_2";
//            v1.Pointer.Value = false;
//            v1.Size.EnumerationValue = Obfuscator.SizeType.EnumValues.eword;
//            v1.Value="param";

//            Obfuscator.BasicBlockType bb1 = func1.BasicBlock.Append();
//            bb1.ID.Value = "ID_12";

//            Obfuscator.InstructionType inst1 = bb1.Instruction.Append();
//            inst1.ID.Value = "ID_3";
//            inst1.RefVars.Value = v1.ID.Value;
//            inst1.StatementType.EnumerationValue = Obfuscator.StatementTypeType.EnumValues.eFullAssignment;
//            inst1.Value = "t1:=param+6";



            // Getting input from InputProvider
            XmlDocument doc = new XmlDocument();
            InputProvider ip = new InputProvider();
            doc = ip.Read(InputType.PseudoCode, PlatformType.x86);

            // Validating XML by Schema
            string error;
            if (!ValidateExchangeXML(doc, out error))
            {
                Console.WriteLine("Document invalid: " + error);
                return;
            }
            else
            {
                Console.WriteLine("Document is valid.");
            }

            // Converting XML to Exchange and performing logical control
            Exchange exch;
            if (!ConvertToExchangeType(doc, out exch, out error))
            {
                Console.WriteLine("Document conversion failed!\n" + error);
                return;
            }
            else
                Console.WriteLine("Document converted successfully.\n");

            // For debugging
            exch.SaveToFile("Exchange1.xml", true);
            
            ILObfuscator.Routine routine = new ILObfuscator.Routine(exch);

                        
            //   ...
            //   doc.SaveToFile("Exchange1.xml", true);
            //
            // Example code to load and save a structure:
            //   Exchange.Exchange2 doc = Exchange.Exchange2.LoadFromFile("Exchange1.xml");
            //   Exchange.VariableType root = doc.Variable.First;
            //   ...
            //   doc.SaveToFile("Exchange1.xml", true);


        }


        protected static bool ValidateExchangeXML(XmlDocument doc2validate, out string error_message)
        {
            bool valid = true;
            System.Xml.Schema.XmlSchemaSet schemas = new System.Xml.Schema.XmlSchemaSet();
            schemas.Add(null, @"Schemas\Exchange.xsd");

            XDocument doc = XDocument.Parse(doc2validate.InnerXml);
            string msg = string.Empty;
            
            doc.Validate(schemas, (o, e) =>
            {
                msg = e.Message;
                valid = false;
            });
            
            error_message = msg;
            
            return valid;
        }


        protected static bool ConvertToExchangeType(XmlDocument doc, out Exchange exch, out string error_message)
        {
            exch = Exchange.LoadFromString(doc.InnerXml);
            error_message = string.Empty;
            foreach (RoutineType routine in exch.Routine)
            {
                foreach (FunctionType function in routine.Function)
                {
                    //Check ID correctness
                    if (!checkIDcorrectness(function.ID.Value, ref error_message))
                        return false;

                    // Collecting IDs for all variables within a function
                    List<string> variable_IDs = new List<string>();
                    if(function.Inputs.Exists && function.Inputs[0].Original.Exists && function.Inputs[0].Original[0].Variable.Exists)
                        foreach (VariableType var in function.Inputs[0].Original[0].Variable)
	    	                variable_IDs.Add(var.ID.Value);
                    if (function.Inputs.Exists && function.Inputs[0].Fake.Exists && function.Inputs[0].Fake[0].Variable.Exists)
                        foreach (VariableType var in function.Inputs[0].Fake[0].Variable)
                            variable_IDs.Add(var.ID.Value);
                    if (function.Outputs.Exists && function.Outputs[0].Original.Exists && function.Outputs[0].Original[0].Variable.Exists)
                        foreach (VariableType var in function.Outputs[0].Original[0].Variable)
                            variable_IDs.Add(var.ID.Value);
                    if (function.Outputs.Exists && function.Outputs[0].Fake.Exists && function.Outputs[0].Fake[0].Variable.Exists)
                        foreach (VariableType var in function.Outputs[0].Fake[0].Variable)
                            variable_IDs.Add(var.ID.Value);
                    if (function.Locals.Exists && function.Locals[0].Original.Exists && function.Locals[0].Original[0].Variable.Exists)
                        foreach (VariableType var in function.Locals[0].Original[0].Variable)
                            variable_IDs.Add(var.ID.Value);
                    if (function.Locals.Exists && function.Locals[0].Fake.Exists && function.Locals[0].Fake[0].Variable.Exists)
                        foreach (VariableType var in function.Locals[0].Fake[0].Variable)
                            variable_IDs.Add(var.ID.Value);

                    // Checking 'Variable ID' for correctness
                    foreach(string var in variable_IDs)
                        if (!checkIDcorrectness(var, ref error_message))
                            return false;

                    // Collecting IDs for Basic Blocks and checking the consistence of Predecessors and Successors.
                    List<string> basicblock_IDs = new List<string>();
                    foreach (BasicBlockType bb in function.BasicBlock)
                    {
                        // Checking 'BasicBlock ID' for correctness
                        if (!checkIDcorrectness(bb.ID.Value, ref error_message))
                            return false;

                        basicblock_IDs.Add(bb.ID.Value);
                        // Checking RefVar ID for variables
                        foreach (InstructionType inst in bb.Instruction)
                        {
                            //Checking 'Instruction ID' for correctness
                            if (!checkIDcorrectness(inst.ID.Value, ref error_message))
                                return false;

                            if (inst.RefVars.Exists())
                            {
                                string[] refvars = inst.RefVars.Value.Split(' ');
                                foreach (string refvar in refvars)
                                    if (!variable_IDs.Contains(refvar))
                                    {
                                        error_message = "\nAn instruction contains reference to a non-existing variable.\nAdditional information:\n- Referenced variable (not found): " + refvar + "\n- Instruction: " + inst.ID.Value + ",\n- Basic Block: " + bb.ID.Value + ",\n- Function: " + function.ID.Value + ".";
                                        return false;
                                    }
                            }
                        }
                    }
                    foreach (BasicBlockType bb in function.BasicBlock)
                    {
                        if (bb.Predecessors.Exists())
                        {
                            string[] predecessors = bb.Predecessors.Value.Split(' ');
                            foreach (string pred in predecessors)
                                if (!basicblock_IDs.Contains(pred))
                                {
                                    error_message = "\n'Predecessors' reference is invalid in the basic block.\nAdditional information:\n- Basic block: " + bb.ID.Value + "\n- Predecessor ID (not found): " + pred + "\n- Function:" + function.ID.Value + ".";
                                    return false;
                                }
                        }
                        if (bb.Successors.Exists())
                        {
                            string[] successors = bb.Successors.Value.Split(' ');
                            foreach (string succ in successors)
                                if (!basicblock_IDs.Contains(succ))
                                {
                                    error_message = "\n'Successors' reference is invalid in the basic block.\nAdditional information:\n- Basic block: " + bb.ID.Value + "\n- Successor ID (not found): " + succ + "\n- Function:" + function.ID.Value + ".";
                                    return false;
                                }
                        }
                    }
                }
            }
            return true;
        }

        protected static bool checkIDcorrectness(string id, ref string error_message)
        {
            if (id.ToUpper() != id)
            {
                error_message = "The unique identifier " + id + " seems not to be uppercase.";
                return false;
            }
            if(!id.StartsWith("ID_"))
            {
                error_message = "The unique identifier " + id + " does not start with ID_";
                return false;
            }
            try
            {
                Guid guid = Guid.Parse(id.Substring(3));
            }
            catch (FormatException fe)
            {
                error_message = "The unique identifier " + id + " is not in a form ID_'GUID'.\nAdditional information: " + fe.Message;
                return false;
            }
            return true;
        }



        [STAThread]
        static int Main(string[] args)
        {

            try
            {
//                Console.WriteLine("Exchange Test Application");
                Example();
                Console.WriteLine("Program has finished successfully.");
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }

            

            //XmlDocument doc = new XmlDocument();
            //InputProvider ip = new InputProvider();
            //doc = ip.Read(InputType.PseudoCode, PlatformType.x86);
            //doc.Save("test4.xml");
            //Console.Write("zzz");
            
        }

    }
}

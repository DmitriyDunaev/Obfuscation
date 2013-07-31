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




            XmlDocument doc = new XmlDocument();
            InputProvider ip = new InputProvider();
            doc = ip.Read(InputType.PseudoCode, PlatformType.x86);
            doc.Save("test2.xml");
            string error;
            if (!ValidateExchangeXML(doc, out error))
            {
                Console.WriteLine("Document invalid: " + error);
                return;
            }
            else
            {
                Console.WriteLine("Document is valid!");
            }

            Exchange exch;
            if (!ConvertToExchangeType(doc, out exch, out error))
            {
                Console.WriteLine("Document conversion failed. Error message: " + error);
                return;
            }

//            Obfuscator.Exchange exch = Obfuscator.Exchange.LoadFromFile("test2.xml");
            
            exch.SaveToFile("Exchange1.xml", true);

                        
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
            foreach (RoutineType routine in exch.Routine)
            {
                foreach (FunctionType function in routine.Function)
                {
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

                    // Collecting IDs for Basic Blocks and checking the consistence of Predecessors and Successors.
                    List<string> basicblock_IDs = new List<string>();
                    foreach (BasicBlockType bb in function.BasicBlock)
                    {
                        basicblock_IDs.Add(bb.ID.Value);
                        // Checking RefVar ID for variables
                        foreach (InstructionType inst in bb.Instruction)
                            if (inst.RefVars.Exists())
                            {
                                string[] refvars = inst.RefVars.Value.Split(' ');
                                foreach (string refvar in refvars)
                                    if (!variable_IDs.Contains(refvar))
                                    {
                                        error_message = "An instruction contains reference to a non-existing variable. Additional information: Instruction ID="+inst.ID.Value+", Basic Block ID=" + bb.ID.Value + ", Function ID=" + function.ID.Value + ").";
                                        return false;
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
                                    error_message = "In a Basic Block (" + bb.ID.Value + ") 'Predecessors' reference is invalid. No predecessor with ID={" + pred + "} found in Function (ID=" + function.ID.Value + ").";
                                    return false;
                                }
                        }
                        if (bb.Successors.Exists())
                        {
                            string[] successors = bb.Successors.Value.Split(' ');
                            foreach (string succ in successors)
                                if (!basicblock_IDs.Contains(succ))
                                {
                                    error_message = "In a Basic Block (" + bb.ID.Value + ") 'Successors' reference is invalid. No successor with ID={" + succ + "} found in Function (" + function.ID.Value + ").";
                                    return false;
                                }
                        }
                    }
                }
            }


            error_message = string.Empty;
            return true;
        }



        [STAThread]
        static int Main(string[] args)
        {

            ILObfuscator.Instruction inst = new ILObfuscator.Instruction();
            inst.ID.setID();

            try
            {
                Console.WriteLine("Exchange Test Application");
                Example();
                Console.WriteLine("OK");
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

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
            if (!Validator.ValidateXML(doc, out error))
            {
                Console.WriteLine("Document invalid: " + error);
                return;
            }
            else
            {
                Console.WriteLine("Document is valid.");
            }

            // Converting XML to Exchange
            Exchange exch = Validator.ConvertXMLToExchangeType(doc);

            // Performing logical control
            if (!Validator.ValidateExchangeType(exch, out error))
            {
                Console.WriteLine("Document is logically incorrect!\n" + error);
                return;
            }
            else
                Console.WriteLine("Document is logically correct.\n");

            // For debugging
            exch.SaveToFile("Exchange1.xml", true);

            // Creating a routine
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




        [STAThread]
        static int Main(string[] args)
        {

            try
            {
                Example();
                Console.WriteLine("Program has finished successfully.");
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }

        }

    }
}

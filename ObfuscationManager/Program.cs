using CommonModule;
using ExchangeFormat;
using System;
using System.Xml;


namespace ObfuscationManager
{
    class Manager
    {
        protected static void Example()
        {
            // Getting input from InputProvider
            XmlDocument doc = new XmlDocument();
            InputProvider ip = new InputProvider();
            doc = ip.Read(InputType.PseudoCode, PlatformType.x86);

            // Validating XML by Schema
            try
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\n\tIMPORTING ROUTINE\n");
                Console.ResetColor();
                Console.Write("Loading XML, checking complience with Exchange.xsd");
                Internal.Validator.ValidateXML(doc);
            }
            catch (Obfuscator.ValidatorException exc)
            {
                Console.WriteLine(exc.Message);
                if (exc.InnerException != null)
                    throw exc.InnerException;
                else throw exc;
            }
            Obfuscator.ILObfuscator.PrintSuccess();
            
            // Converting XML to Exchange
            Exchange exch = Exchange.LoadFromString(doc.InnerXml);

            // For debugging
            exch.SaveToFile("Exchange1.xml", true);

            // Sending Exchange format to obfuscator
            Obfuscator.ILObfuscator.Obfuscate(exch);



            // Validating XML by Schema
            try
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\n\tEXPORTING ROUTINE\n");
                Console.ResetColor();
                Console.Write("Generating XML, checking complience with Exchange.xsd");
                Console.WriteLine(" . COMING SOON");
            }
            catch (Obfuscator.ValidatorException exc)
            {
                Console.WriteLine(exc.Message);
                if (exc.InnerException != null)
                    throw exc.InnerException;
                else throw exc;
            }
            //Obfuscator.ILObfuscator.PrintSuccess();



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
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Program has finished successfully.\n");
                Console.ResetColor();
                return 0;
            }
            catch (Exception e)
            {
                Obfuscator.Logging.WriteException(e);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\nProgram failed!");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(string.Format("- Message: {0}\n- Target: [{1}.{2}]\nYou can find the full stack trace in a log file.\n", 
                    e.Message, e.TargetSite.DeclaringType, e.TargetSite.Name));
                Console.ResetColor();
//                Console.WriteLine(e.StackTrace);
                return 1;
            }
        }
    }
}

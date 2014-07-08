using CommonModule;
using ExchangeFormat;
using System;
using System.Xml;


namespace ObfuscationManager
{
    class Manager
    {
        protected static void Run()
        {
            XmlDocument doc;    // XML Document used for data exchange between modules
            Exchange exch;      // Exchange type
            try
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\n\tIMPORTING ROUTINE\n");
                Console.ResetColor();
                Console.Write("Loading XML, checking complience with Exchange.xsd");
                string pathToPC = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pseudocode", System.Configuration.ConfigurationManager.AppSettings["PseudoCode"]);
                doc = ExportImport.ImportXmlNew(InputType.PseudoCode, PlatformType.x86, pathToPC);
                exch = ExportImport.XmlToExchange(doc);
            }
            catch (Obfuscator.ValidatorException exc)
            {
                if (exc.InnerException != null)
                    throw exc.InnerException;
                else throw exc;
            }
            Obfuscator.ILObfuscator.PrintSuccess();

            // For debugging
            exch.SaveToFile("Import.xml", true);

            // Sending Exchange format to obfuscator
            Obfuscator.ILObfuscator.Obfuscate(ref exch);


            // Validating XML by Schema
            try
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\n\tEXPORTING ROUTINE\n");
                Console.ResetColor();
                Console.Write("Generating XML, checking complience with Exchange.xsd");
                doc = ExportImport.ExchangeToXml(exch);
                Obfuscator.ILObfuscator.PrintSuccess();
                
                Console.Write("Converting XML to platform-dependent assembly");
                //ExportImport.XmlToAsm(doc, PlatformType.x86);
                Console.WriteLine(" . . . . . CANCELLED\n");

                Console.Write("Converting Exchange to platform-dependent assembly");
                string asm = ExportImport.ExchangeToAsm(exch, PlatformType.x86);
                Obfuscator.Logging.WriteTextFile(asm, "ASM");
                Console.WriteLine(". . . IMPLEMENTING\n");
            }
            catch (Obfuscator.ValidatorException exc)
            {
                if (exc.InnerException != null)
                    throw exc.InnerException;
                else throw exc;
            }
            //Obfuscator.ILObfuscator.PrintSuccess();
        }

        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                Run();
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

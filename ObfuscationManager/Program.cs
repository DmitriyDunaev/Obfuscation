using ExchangeFormat;
using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;


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
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n\n\tINTERMEDIATE LEVEL OBFUSCATOR v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\n\n\tIMPORTING ROUTINE\n");
                Console.ResetColor();
                Console.Write("Getting XML from platform-dependent module");
                string pathToPC = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pseudocode", System.Configuration.ConfigurationManager.AppSettings["PseudoCode"]);
                doc = Platform_x86.PseudoCode.GetTAC(pathToPC);
                Obfuscator.ILObfuscator.PrintSuccess();
                Console.Write("Performing formal control");
                ValidateXml(doc);
                exch = Exchange.LoadFromString(doc.InnerXml);
            }
            catch (Exception exc)
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

                Console.Write("Performing formal control");
                doc.LoadXml(exch.SaveToString(true));
                doc.Save("export.xml");
                ValidateXml(doc);
                Obfuscator.ILObfuscator.PrintSuccess();
                
                Console.Write("Sending XML to platform-dependent module");
                Obfuscator.ILObfuscator.PrintSuccess();

                Console.Write("Saving platform-dependent assembly");
                string asm = Platform_x86.Assembler.GetPlatformDependentCode(doc);
                Services.Logging.WriteTextFile(asm, "ASM");
                Obfuscator.ILObfuscator.PrintSuccess();
            }
            catch (Exception exc)
            {
                if (exc.InnerException != null)
                    throw exc.InnerException;
                else throw exc;
            }
        }

        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                Run();
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("\nProgram has finished successfully.\n");
                Console.ResetColor();
                return 0;
            }
            catch (Exception e)
            {
                Services.Logging.WriteException(e);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\nProgram failed!");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(string.Format("- Message: {0}\n- Target: [{1}.{2}]\nYou can find the full stack trace in a log file.\n", 
                    e.Message, e.TargetSite.DeclaringType, e.TargetSite.Name));
                Console.ResetColor();
                Console.WriteLine(e.StackTrace);
                return 1;
            }
        }


        /// <summary>
        /// Validates an XML document against Exchange.xsd schema
        /// </summary>
        /// <param name="doc2validate">XML document to be validated</param>
        private static void ValidateXml(XmlDocument doc2validate)
        {
            System.Xml.Schema.XmlSchemaSet schemas = new System.Xml.Schema.XmlSchemaSet();
            schemas.Add(null, @"Scheme\Exchange.xsd");
            try
            {
                XDocument doc = XDocument.Parse(doc2validate.InnerXml);
                doc.Validate(schemas, (o, e) =>
                {
                    throw new Exception(e.Message);
                });
            }
            catch (Exception ex)
            {
                throw new Exception("XML could not be validated! It is not well-formed or does not comply with XSD.", ex);
            }
        }
    }
}

using CommonModule;
using ExchangeFormat;
using Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;


namespace ObfuscationManager
{
    public static class ExportImport
    {

        [DllImport("CommonModule.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern XmlDocument ReadChar(string fileName, int memFlag, long defaultN);

        [MarshalAs(UnmanagedType.LPStr)]
        private static string path;

        /// <summary>
        /// Imports routine data from low-level a platform-dependent module
        /// </summary>
        /// <param name="input">Input type</param>
        /// <param name="platform">Platform type</param>
        /// <param name="path2PC">Full path to pseudocode</param>
        /// <returns>XML document</returns>
        //public static XmlDocument ImportXml(InputType input, PlatformType platform, string path2PC)
        //{
            
        //    StringBuilder sb = new StringBuilder();
        //    sb.Append(path2PC);
        //    XmlDocument doc = new XmlDocument();
        //    InputProvider ip = new InputProvider();
        //    doc = ip.ReadString(input, platform, path2PC);
        //    ValidateXml(doc);
        //    return doc;
        //}

        public static XmlDocument ImportXmlNew(InputType input, PlatformType platform, string path2PC)
        {
            XmlDocument doc = new XmlDocument();
            if (platform == PlatformType.x86 && input == InputType.PseudoCode)
                doc = Platform_x86.PseudoCode.GetTAC(path2PC);
            ValidateXml(doc);
            return doc;
        }


        /// <summary>
        /// Converts XML document to Exchange type
        /// </summary>
        /// <param name="doc">XML document to be converted</param>
        /// <returns>Exchange type</returns>
        public static Exchange XmlToExchange(XmlDocument doc)
        {
            return Exchange.LoadFromString(doc.InnerXml);
        }


        /// <summary>
        /// Converts Exchange type to XML document
        /// </summary>
        /// <param name="exch">Exchange type</param>
        /// <returns>XML documnet</returns>
        public static XmlDocument ExchangeToXml(Exchange exch)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(exch.SaveToString(true));
            doc.Save("export.xml");
            ValidateXml(doc);
            return doc;
        }


        /// <summary>
        /// Converts XML document to a low-level platform-dependent Assembly instruction set
        /// </summary>
        /// <param name="doc">XML document</param>
        /// <param name="platform">Platform type</param>
        public static void XmlToAsm(XmlDocument doc, PlatformType platform)
        {
            //TODO: Convert XML document to low-level Assembly instruction set
        }


        /// <summary>
        /// Validates an XML document against Exchange.xsd schema
        /// </summary>
        /// <param name="doc2validate">XML document to be validated</param>
        private static void ValidateXml(XmlDocument doc2validate)
        {
            System.Xml.Schema.XmlSchemaSet schemas = new System.Xml.Schema.XmlSchemaSet();
            schemas.Add(null, @"Schemas\Exchange.xsd");
            try
            {
                XDocument doc = XDocument.Parse(doc2validate.InnerXml);
                doc.Validate(schemas, (o, e) =>
                {
                    throw new Obfuscator.ValidatorException(e.Message);
                });
            }
            catch (Exception ex)
            {
                throw new Obfuscator.ValidatorException("XML could not be validated! It is not well-formed or does not comply with XSD.", ex);
            }
        }

        internal static string ExchangeToAsm(Exchange exch, PlatformType platformType)
        {
            Routine routine = (Routine)exch;
            return Platform_x86.Assembler.GetAssemblyFromTAC(routine);
        }
    }
}

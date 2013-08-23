using CommonModule;
using ExchangeFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;


namespace ObfuscationManager
{
    public static class ExportImport
    {
        public static XmlDocument ImportRoutineXML(InputType input, PlatformType platform)
        {
            XmlDocument doc = new XmlDocument();
            InputProvider ip = new InputProvider();
            doc = ip.Read(input, platform);
            ValidateExchangeXML(doc);
            return doc;
        }


        public static Exchange XmlToExchange(XmlDocument doc)
        {
            return Exchange.LoadFromString(doc.InnerXml);
        }


        public static XmlDocument ExchangeToXml(Exchange exch)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(exch.SaveToString(true));
            return doc;
        }


        public static void ExportRoutineXML(XmlDocument doc, PlatformType platform)
        {
            //ValidateExchangeXML(doc);
            //TODO: Send XML to Common Module for conversion to assembly
        }

        /// <summary>
        /// Validates an XML document against the Exchange.xsd schema
        /// </summary>
        /// <param name="doc2validate">XML document to be validated</param>
        private static void ValidateExchangeXML(XmlDocument doc2validate)
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
    }
}

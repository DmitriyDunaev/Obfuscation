using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonModule;
using System.Xml;

namespace ObfuscationManager
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlDocument doc = new XmlDocument();
            InputProvider ip = new InputProvider();
            doc = ip.Read(InputType.PseudoCode, PlatformType.x86);
            Console.Write("zzz");
            
        }

        XmlDocument test(string zz)
        {
            //XmlDocument doc = new XmlDocument();
            //doc.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Routine xsi:noNamespaceSchemaLocation=\"CFG_Schema.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">	<BasicBlock Out=\"ID_5 ID_2\" ID=\"ID_0\">	<Instruction Type=\"original\" Label=\"\">result := 9</Instruction>	<Instruction Type=\"original\" Label=\"\">if  csudajo goto LABEL_3</Instruction></BasicBlock></Routine>");
            return new XmlDocument();
        }
    }
}

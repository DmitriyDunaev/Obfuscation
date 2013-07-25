// This is the main DLL file.

#include "CommonModule.h"



Xml::XmlDocument^ CommonModule::InputProvider::Read (InputType it, PlatformType pt)
{
	Xml::XmlDocument^ doc = gcnew Xml::XmlDocument;
	doc->LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Routine xsi:noNamespaceSchemaLocation=\"CFG_Schema.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">	<BasicBlock Out=\"ID_5 ID_2\" ID=\"ID_0\">	<Instruction Type=\"original\" Label=\"\">result := 9</Instruction>	<Instruction Type=\"original\" Label=\"\">if  csudajo goto LABEL_3</Instruction></BasicBlock></Routine>");
	return doc;
	
}
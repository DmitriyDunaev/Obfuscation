// This is the main DLL file.

//#define ORIGINAL
//#define TEST
//#define SIMPLE

using namespace std;

#include <fstream>
#include "CommonModule.h"

Xml::XmlDocument^ CommonModule::InputProvider::Read (InputType it, PlatformType pt)
{
	Xml::XmlDocument^ doc = gcnew Xml::XmlDocument;
	Reader r1;
	//doc->LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Routine xsi:noNamespaceSchemaLocation=\"CFG_Schema.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">	<BasicBlock Out=\"ID_5 ID_2\" ID=\"ID_0\">	<Instruction Type=\"original\" Label=\"\">result := 9</Instruction>	<Instruction Type=\"original\" Label=\"\">if  csudajo goto LABEL_3</Instruction></BasicBlock></Routine>");
	std::string str = r1.DoStuff();
	String ^ s = gcnew String(str.c_str());
	doc->LoadXml(s);
	//cout << str.c_str() << endl;
	return doc;
	
}

//#define DEB

//#define CIN

#include <iostream>

#include "CommonModule.h"
#include "exchange.h"
#include "assistant.h"
#include "parser.h"

string CommonModule::Reader::DoStuff()
{

    Routine c;

    std::string out;
	char in[1000];
    list<Line> lines;

    stringstream ss;
	fstream fin;
	fin.open("Pseudocode.txt", fstream::in );
	if( !fin.is_open() )
		cout << "File not found!" << endl;

#ifdef CIN
    while (!cin.eof())
    {
        getline(cin, in);
#else
	while (!fin.eof())
    {
        fin.getline(in, 1000);
		string s(in);
#endif
        lines.push_back(s);
    }
	
	fin.close();

    Assistant Reader(&lines, &c);

    Reader.preproc();

    Reader.work( Reader.lines->begin(), Reader.lines->end());

    Reader.setconnentions();

	Reader.setfunctions();

	stringstream s;
    Reader.printlist(s);
	//cout << s.str();

	fstream f("Debug.xml", fstream::trunc | fstream::out );
	f << s.str();
	f.close();

	return s.str();



}

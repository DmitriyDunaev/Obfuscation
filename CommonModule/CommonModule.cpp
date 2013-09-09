// This is the main DLL file.

//#define ORIGINAL
//#define TEST
//#define SIMPLE

using namespace std;

#include <fstream>
#include "CommonModule.h"

Xml::XmlDocument^ CommonModule::InputProvider::ReadChar (InputType it, PlatformType pt, char* path_to_pseudocode)
{
	ReadString(it, pt, path_to_pseudocode);
}

Xml::XmlDocument^ CommonModule::InputProvider::ReadString (InputType it, PlatformType pt, string path_to_pseudocode)
{
	Xml::XmlDocument^ doc = gcnew Xml::XmlDocument;
	Reader r1;
	std::string str = r1.DoStuff(path_to_pseudocode);
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

string CommonModule::Reader::DoStuff(string path_to_pseudocode)
{

    Routine c;

    std::string out;
	char in[1000];
    list<Line> lines;

    stringstream ss;
	fstream fin;
	fin.open(path_to_pseudocode, fstream::in );
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

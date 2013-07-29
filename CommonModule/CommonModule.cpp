// This is the main DLL file.

using namespace std;

#include "CommonModule.h"

Xml::XmlDocument^ CommonModule::InputProvider::Read (InputType it, PlatformType pt)
{
	Xml::XmlDocument^ doc = gcnew Xml::XmlDocument;
	Reader r1;
	r1.DoStuff();
	//doc->LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Routine xsi:noNamespaceSchemaLocation=\"CFG_Schema.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">	<BasicBlock Out=\"ID_5 ID_2\" ID=\"ID_0\">	<Instruction Type=\"original\" Label=\"\">result := 9</Instruction>	<Instruction Type=\"original\" Label=\"\">if  csudajo goto LABEL_3</Instruction></BasicBlock></Routine>");
	std::string str = r1.DoStuffNop();
	String ^ s = gcnew String(str.c_str());
	doc->LoadXml(s);
	cout << str.c_str() << endl;
	return doc;
	
}

//#define DEB
#define DEB2

//#define CIN

#include <iostream>

#include "CommonModule.h"
#include "Interface.h"
#include "assistant.h"
#include "parser.h"

string CommonModule::Reader::DoStuffNop()
{
    Blocks c;

#ifdef DEB

    //Parser p("( v5 + 0x70 ) == 16", c);
    Parser p("2 = b + c", c.back(), c.getVars());
    //Parser p("cumo - 4 * ( 3 + 12 ) >= a + b * c + d", c);
    //Parser p("a + b >= c - 12", c);
    p.work();
    std::cout << "STR: " << p.str() << std::endl;
    p.print();
    c.dump();

	// Random.

    return 0;

#else

    std::string out, in;
    list<Line> lines;

    stringstream ss;
   /* ss << "for ( i = 0; i; ++i )\n{\n*(_DWORD *)v6 = dword_405008;\n*v5 = dword_402000;\nv6";
    ss << " += 4;\n}\nreturn *(_DWORD *)v3;";*/
//
	ss << " int sub_kaki(a, b)\n";
	ss << " {\n";
    ss << " result = 10;\n";
    ss << " if ( csudajo || azis && blabla)\n";
    ss << " {\n";
    ss << " igaz = 1;\n";
    ss << " hamis = 0;\n";
    ss << " }\n";
    ss << " else\n";
    ss << " {\n";
    ss << " igaz = 0;\n";
    ss << " hamis = 1;\n";
    ss << " }\n";
    ss << " return result;\n";
	ss << " }\n";
	ss << " kaki.";


#ifdef CIN
    while (!cin.eof())
    {
        getline(cin, in);
#else
    while (!ss.eof())
    {
        getline(ss, in);
#endif
        lines.push_back(in);
    }
#ifdef DEB2
    for ( list<Line>::iterator i = lines.begin(); i != lines.end(); ++i)
    {
        cout << i->gets() << endl;
    }
#endif

    Assistant Reader(&lines, &c);

    Reader.preproc();
#ifdef DEB2
    for ( list<Line>::iterator i = lines.begin(); i != lines.end(); ++i)
    {
        cout << i->gets() << endl;
    }
#endif

    Reader.work( Reader.lines->begin(), Reader.lines->end());

    Reader.setconnentions();

    FILE *f = fopen("test2.xml", "w");

	stringstream s;
    Reader.printlist(s);
	cout << s;
	fputs(s.str().c_str(), f);


    fclose(f);
	return s.str();


#endif

}

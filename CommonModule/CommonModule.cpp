// This is the main DLL file.

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

ss << "int __cdecl sub_401334()\n{\n  sub_401980();\n  return sub_401368(777, 1);\n}\n";
ss << "int __cdecl sub_401368(int a1, int a2)\n{\n  char v3; // [sp+8h] [bp-38h]@1\n";
ss << "  int v4; // [sp+1Ch] [bp-24h]@1\n  char *v5; // [sp+30h] [bp-10h]@1\n";
ss << "  int i; // [sp+34h] [bp-Ch]@1\n  int *v7; // [sp+38h] [bp-8h]@1\n";
ss << "  char *v8; // [sp+3Ch] [bp-4h]@1\n  int v9; // [sp+48h] [bp+8h]@1\n\n";
ss << "  v9 = 3 * a2 + a1;\n  v8 = &v3;\n  v5 = &v3;\n  v7 = &v4;\n";
ss << "  for ( i = 1; i <= 4; ++i )\n  {\n    a2 += i;\n    *(_DWORD *)v8 = v9;\n";
ss << "    *(_DWORD *)v7 = a2;\n    v8 += 4;\n    ++v7;\n  }\n  return *(_DWORD *)v5;\n}\n";


	//ss << " int sub_fnct(a, b)\n";
	//ss << " {\n";
	//ss << " int *hades;\n";
	//ss << " signed int poseidon;\n";
	//ss << " unsigned long *mars;\n";
	//ss << " char zeus;\n";
	//ss << " if ( x > y );\n";
	//ss << " return x;\n";
	//ss << " c = y + z;\n";
	//ss << " return y;\n";
	//ss << " }\n";
	//ss << " int sub_masik(ca, b)\n";
	//ss << " {\n";
	//ss << " xa = ya;\n";
	//ss << " return xa;\n";
	//ss << " ca = ya + az;\n";
	//ss << " return ya;\n";
	//ss << " }";


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


#endif

}

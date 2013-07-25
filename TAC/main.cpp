//#define DEB
#define DEB2

//#define CIN

#include <iostream>

#include "assistant.h"
#include "parser.h"

using namespace std;

int main (void)
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

    return 0;

#else

    std::string out, in;
    list<Line> lines;

    stringstream ss;
//    ss << "int __cdecl sub_401334()\n{\nchar v1; // [sp+8h] [bp-38h]@1\nint v2;";
//    ss << "// [sp+1Ch] [bp-24h]@1\nchar *v3; // [sp+30h] [bp-10h]@1\nint i; // [sp+34h] [bp-Ch]@1\nint *v5;";
//    ss << " // [sp+38h] [bp-8h]@1\nchar *v6; // [sp+3Ch] [bp-4h]@1\n\nsub_401960();\nv6 = &v1;\nv3 = &v1;\nv5";
//    ss << " = &v2;\nfor ( i = 0; i; ++i )\n{\n*(_DWORD *)v6 = dword_405008;\n*v5 = dword_402000;\nv6";
//    ss << " += 4;\n++v5;\n}\nreturn *(_DWORD *)v3;\n}\n// 402000: using guessed type int dword_402000;\n//";
//    ss << " 405008: using guessed type int dword_405008;\n" ;

    ss << " result = 9;\n";
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
    ss << " return result;";



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

    Assistant Miss_Candy(&lines, &c);

    Miss_Candy.preproc();
#ifdef DEB2
    for ( list<Line>::iterator i = lines.begin(); i != lines.end(); ++i)
    {
        cout << i->gets() << endl;
    }
#endif

    Miss_Candy.work( Miss_Candy.lines->begin(), Miss_Candy.lines->end());

    Miss_Candy.setconnentions();

    ofstream f ("test.xml", ios::out);


    Miss_Candy.printlist(f);

    f.close();
    return 0;

#endif

}





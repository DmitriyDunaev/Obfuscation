#ifndef ASSISTANT_H
#define ASSISTANT_H



#include "instructionscontainer.h"
#include "line.h"
#include "labelgenerator.h"
#include "parser.h"
#include "label.h"
#include "call.h"
#include "blocks.h"
#include "functions.h"
#include "unc_jump.h"
#include "cond_jump.h"
#include "fakeexitblock.h"


using namespace std;

//This is the Translator assistant, wich translates the
//pseudo-code to TAC a.k.a. 3AC

class Assistant
{
public:
    Assistant(list<Line>* lines, Routine* b): lines(lines), rtn(b) { cnt = (rtn->back())->back(); }
    list<Line>* lines;
    Routine* rtn;
    CInstructionsContainer* cnt;

    LabelGenerator l;

    void preproc();
    void work(list<Line>::iterator beg , list<Line>::iterator en );
    void setconnentions() { rtn->setconnections(&l); rtn->setjumps(); }

	void addinput(string str, string type, bool in);
	bool checkdecls(string str, bool in);

	void setfunctions();

	void parse(string* s);
    void getfor(string from, string *s1, string *s2, string *s3);
    void getcond(string from, string *s1);

    void setiters(list<Line>::iterator* a, list<Line>::iterator* b, list<Line>::iterator* i, bool inc = true);
    void newblock();
    void generateif(string deal, string label);
    void generategoto(string label);
    void generatelabel(string label);

    void paint();

    void printlist(stringstream& s)
    {
        s << "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";
        s << "<Routine Description=\"Some routine\" xsi:noNamespaceSchemaLocation=\"Schemas\\Exchange.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\n";
        rtn->dump(s);


        s << "</Routine>";
    }
};

#endif // ASSISTANT_H

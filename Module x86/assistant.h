#ifndef ASSISTANT_H
#define ASSISTANT_H



#include "instructionscontainer.h"
#include "line.h"
#include "labelgenerator.h"
#include "parser.h"
#include "label.h"
#include "call.h"
#include "blocks.h"
#include "unc_jump.h"
#include "cond_jump.h"
#include "fakeexitblock.h"


using namespace std;

//This is the Translator assistant, wich translates the
//pseudo-code to TAC a.k.a. 3AC

class Assistant
{
public:
    Assistant(list<Line>* lines, Blocks* b): lines(lines), bl(b) { cnt = (bl->blocks).back(); }
    list<Line>* lines;
    Blocks* bl;
    CInstructionsContainer* cnt;

    LabelGenerator l;

    void preproc();
    void work(list<Line>::iterator beg , list<Line>::iterator en );
    void setconnentions() { bl->setconnections(); }


    void getfor(string from, string *s1, string *s2, string *s3);
    void getwhile(string from, string *s1);
    void getif(string from, string *s1);

    void setiters(list<Line>::iterator* a, list<Line>::iterator* b, list<Line>::iterator* i, bool inc = true);
    void newblock();
    void generateif(string deal, string label);
    void generategoto(string label);
    void generatelabel(string label);

    void paint();

    void printlist(ofstream& s)
    {
        s << "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";
        s << "<Routine xsi:noNamespaceSchemaLocation=\"CFG_Schema.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\n";
        bl->dump(s);


        s << "</Routine>";
    }
};

#endif // ASSISTANT_H

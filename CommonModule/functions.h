#pragma once

#include "blocks.h"

class Routine
{
    std::list<Function*> functs;

    int id_deal;

public:
    Routine() { 
		UUID i, j;
		UuidCreate( &i );
		UuidCreate( &j );
		functs.push_back( new Function(i, "noname", j) ); }
    ~Routine() { clear(); }
    void clear ()
    { for ( std::list<Function*>::iterator i = functs.begin(); i != functs.end(); ++i )
    (*i)->clear(); }

	int get_id() { return id_deal++; }

    Function* back() { return functs.back(); }
	void push_back( Function* f ) { functs.push_back( f ); }
    void cleanup();

    void setconnections(LabelGenerator* l)
	{ for ( std::list<Function*>::iterator i = functs.begin(); i != functs.end(); ++i )
    (*i)->setconnections(l); }

    void dump(stringstream& s)
    {
        for ( std::list<Function*>::iterator i = functs.begin(); i != functs.end(); ++i )
		{
			s << "<Function CalledFrom=\"ExternalOnly\" ID=\"";
			s << (*i)->getid();
			s << "\" ExternalLabel=\"";
			s << (*i)->getname();
			s << "\">\n";
            (*i)->dump(s);
			s << "</Function>\n";
		}
    }
};



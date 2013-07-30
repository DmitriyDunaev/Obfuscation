#ifndef BLOCKS_H
#define BLOCKS_H


#define DEBUG

using namespace std;

#include <list>
#include "instructionscontainer.h"
#include "labelgenerator.h"
#include "fakeexitblock.h"

class Function
{
    std::list<CInstructionsContainer*> blocks;
    CVariables Vars;
	UUID id;
	string name;
public:
    Function(UUID i, string n, UUID j) : id(i), name(n) { blocks.push_back( new CInstructionsContainer(j) ); }
    ~Function() { clear(); }

	string getname() { return name; }
	string getid()
	{
		stringstream tmp;
		tmp.flags( stringstream::hex );
		tmp << id.Data1 << "-" << id.Data2 << "-" << id.Data3 << "-" << id.Data4[0] << id.Data4[1] << "-";
		tmp.width(2);
		tmp.flags();
		tmp << id.Data4[2] << id.Data4[3] << id.Data4[4] << id.Data4[5] << id.Data4[6] << id.Data4[7];
		string ret = tmp.str();
		return ret;
	}

    void clear ()
    { for ( std::list<CInstructionsContainer*>::iterator i = blocks.begin(); i != blocks.end(); ++i )
    (*i)->clear(); }
	bool empty() { return blocks.empty(); }

    CInstructionsContainer* back() { return blocks.back(); }
    void cleanup();
    void setconnections(LabelGenerator* l);

    CInstructionsContainer* findlabel( int i );

    CVariables&  getVars();

    friend class Assistant;

    void dump(stringstream& s)
    {
        for ( std::list<CInstructionsContainer*>::iterator i = blocks.begin(); i != blocks.end(); ++i )
        {
            (*i)->dump(s);
		}
		Vars.dump();
    }
};

#endif // BLOCKS_H

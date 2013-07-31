#ifndef BLOCKS_H
#define BLOCKS_H


#define DEBUG

using namespace std;

#include <list>
#include "instructionscontainer.h"
#include "labelgenerator.h"
#include "fakeexitblock.h"
#include "line.h"

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
		tmp << id.Data1 << "-" << id.Data2 << "-" << id.Data3 << "-" << (int)id.Data4[0] << (int)id.Data4[1] << "-";
		tmp.width(2);
		tmp.flags();
		tmp << std::setfill('0');
		tmp << (int)id.Data4[2] << (int)id.Data4[3] << (int)id.Data4[4] <<(int) id.Data4[5] << (int)id.Data4[6] << (int)id.Data4[7];
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
	void setjumps();

    CInstructionsContainer* findlabel( int i );
	CInstructionsContainer* findjump( int i );

    CVariables&  getVars();

    friend class Assistant;

    void dump(stringstream& s)
    {
		Vars.dump(s);
        for ( std::list<CInstructionsContainer*>::iterator i = blocks.begin(); i != blocks.end(); ++i )
        {
            (*i)->dump(s);
		}
    }
};

#endif // BLOCKS_H

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
	string getid();

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

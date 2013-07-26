#ifndef BLOCKS_H
#define BLOCKS_H


#define DEBUG

using namespace std;

#include <list>
#include "instructionscontainer.h"

#include "fakeexitblock.h"

class Blocks
{
    std::list<CInstructionsContainer*> blocks;
    CVariables Vars;

    int id_deal;

public:
    Blocks() : id_deal(1) { blocks.push_back(new CInstructionsContainer(0)); }
    ~Blocks() { clear(); }
    void clear ()
    { for ( std::list<CInstructionsContainer*>::iterator i = blocks.begin(); i != blocks.end(); ++i )
    (*i)->clear(); }
    void push_back (CInstructionsContainer*);

    CInstructionsContainer& back() { return *blocks.back(); }
    void cleanup();
    void setconnections();

    CInstructionsContainer* findlabel( int i );

    CVariables&  getVars();

    friend class Assistant;

    void dump(stringstream& s)
    {
#ifdef DEBUG
        for ( std::list<CInstructionsContainer*>::iterator i = blocks.begin(); i != blocks.end(); ++i )
            (*i)->dump(s);
        //Vars.dump();
#endif
    }
};

#endif // BLOCKS_H

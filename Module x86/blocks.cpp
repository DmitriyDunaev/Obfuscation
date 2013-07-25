#include "blocks.h"

CVariables&  Blocks::getVars()
{
    return Vars;
}

void Blocks::cleanup()
{
    for ( std::list<CInstructionsContainer*>::iterator i = blocks.begin(); i != blocks.end(); ++i)
        if ((*i)->empty())
        {
            std::list<CInstructionsContainer*>::iterator j = i;
            --i;
            delete *j;
            blocks.erase(j);
            ++i;
        }
}



void Blocks::setconnections()
{
    id_deal++;
    blocks.push_back(new CInstructionsContainer(id_deal));
    blocks.back()->push_back( new FakeExitBlock );

    for ( list<CInstructionsContainer*>::iterator i = blocks.begin(); i != blocks.end(); ++i)
    {
        int lab = (*i)->back()->geti();
        if ( lab != -1 && !(*i)->back()->islabel() )
        {
            CInstructionsContainer* succ = findlabel( lab );
            (*i)->successors.push_back( succ );
            succ->predecessors.push_back( *i );
        }
        if ( !(*i)->back()->isuncjmp() && !(*i)->back()->isexit() )
        {
            ++i;
            list<CInstructionsContainer*>::iterator j = i;
            --i;
            (*i)->successors.push_back( (*j) );
            (*j)->predecessors.push_back( *i );
        }
    }

}

CInstructionsContainer* Blocks::findlabel( int lab )
{
    for ( list<CInstructionsContainer*>::iterator i = blocks.begin(); i != blocks.end(); ++i)
    {
        if ( (*i)->front()->geti() == lab && (*i)->front()->islabel())
            return *i;
    }
    return NULL;
}

#include "blocks.h"

CVariables&  Blocks::getVars()
{
    return Vars;
}

void Blocks::cleanup()
{
	std::list<CInstructionsContainer*>::iterator i = blocks.begin();
    while ( i != blocks.end() )
	{
			std::cout << "Cleanup done" << std::endl;
			if ( (*i)->empty() )
			{
				std::list<CInstructionsContainer*>::iterator j = i;
				++i;
				delete *j;
				blocks.erase(j);
			}
			else
				++i;
	}
}



void Blocks::setconnections()
{
    id_deal++;
    blocks.push_back(new CInstructionsContainer(id_deal));
    blocks.back()->push_back( new FakeExitBlock );

	list<CInstructionsContainer*>::iterator i = blocks.begin();

    while (  i != blocks.end() && !(*i)->back()->isexit())
    {
        int lab = (*i)->back()->geti();
        if ( lab != -1 && !(*i)->back()->islabel())
        {
            CInstructionsContainer* succ;
			//succ = findlabel( lab );
			succ = *i;
            (*i)->successors.push_back( succ );
            succ->predecessors.push_back( *i );
        }
        if ( !(*i)->back()->isuncjmp())
        {
			list<CInstructionsContainer*>::iterator j = i;
            ++i;
            (*i)->predecessors.push_back( (*j) );
            (*j)->successors.push_back( *i );
        }else
			++i;
    }

}

CInstructionsContainer* Blocks::findlabel( int lab )
{
    for ( list<CInstructionsContainer*>::iterator i = blocks.begin(); i != blocks.end(); ++i)
    {
        if ( (*i)->front()->geti() == lab && (*i)->front()->islabel() || 1)
            return *i;
    }
    return NULL;
}

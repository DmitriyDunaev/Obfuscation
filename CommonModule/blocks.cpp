#include "blocks.h"
#include "objbase.h"

CVariables&  Function::getVars()
{
    return Vars;
}

void Function::cleanup()
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

void Function::setconnections(LabelGenerator* l)
{   
	UUID id = l->getid();
    blocks.push_back(new CInstructionsContainer(id));
    blocks.back()->push_back( new FakeExitBlock );
    blocks.back()->push_back( new Line("Fake Exit Block") );

	list<CInstructionsContainer*>::iterator i = blocks.begin();

    while (  i != blocks.end())
    {
		if ( !(*i)->empty() ) {
        int lab = (*i)->back()->geti();
        if ( lab != -1 && (!(*i)->back()->islabel() ))
        {
            CInstructionsContainer* succ;
			succ = findlabel( lab );
			if (succ != NULL )
			{
				(*i)->succpush_back( succ );
				succ->predpush_back( *i );
			}
        }
        if ( lab != -1 && !(*i)->back()->isuncjmp())
        {
			list<CInstructionsContainer*>::iterator j = i;
            ++i;
            (*i)->predpush_back( (*j) );
            (*j)->succpush_back( *i );
        }else
			++i;
		} else
			++i;
	}

}

void Function::setjumps()
{   
	list<CInstructionsContainer*>::iterator i = blocks.begin();

    while (  i != blocks.end())
    {
		if ( !(*i)->empty() )
		{
			int lab = (*i)->front()->geti();
			if ( lab != -1 && ( (*i)->front()->islabel() || (*i)->front()->isexit() ) )
			{
				CInstructionsContainer* succ;
				succ = findjump( lab );
				while (succ != NULL )
				{
					succ->back()->settarget( (*i)->frontplus() );
					succ->back()->seti( -1 );
					succ = findjump( lab );
				}
			(*i)->erasefront();
			}
			++i;
		} else
			++i;
	}

}

CInstructionsContainer* Function::findjump( int lab )
{
    for ( list<CInstructionsContainer*>::iterator i = blocks.begin(); i != blocks.end(); ++i)
    {
        if ( (*i)->back()->geti() == lab && !(*i)->back()->islabel())
            return *i;
    }
    return NULL;
}

CInstructionsContainer* Function::findlabel( int lab )
{
    for ( list<CInstructionsContainer*>::iterator i = blocks.begin(); i != blocks.end(); ++i)
    {
        if ( (*i)->front()->geti() == lab && ( (*i)->front()->islabel() || (*i)->front()->isexit()))
            return *i;
    }
    return NULL;
}

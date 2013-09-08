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
			//std::cout << "Cleanup done" << std::endl;
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
    blocks.back()->push_back( new Unc_jump("Fake Exit Block", -3) );

	list<CInstructionsContainer*>::iterator i = blocks.begin();

	while ( i != blocks.end() )
	{
		if ( (*i)->onlylabel() )
		{
			list<CInstructionsContainer*>::iterator j = i;
			++i;
			(*i)->push_front( (*j)->front() );
			blocks.remove( (*j) );
		}
		else
			++i;
	}

	i = blocks.begin();

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
        if ( !(*i)->back()->isuncjmp() && !(*i)->back()->isexit() && !(*i)->front()->isexit())
        {
			list<CInstructionsContainer*>::iterator j = i;
            ++i;
			if ( !(*i)->back()->isexit() )
			{
				(*i)->predpush_back( (*j) );
				(*j)->succpush_back( *i );
			}
        }else
			++i;
		} else
			++i;
	}

}

string Function::getid()	
{
	stringstream tmp;
	tmp.flags( stringstream::hex | stringstream::uppercase);
	tmp.fill('0');
	tmp.width(8);
	tmp << id.Data1 << "-";
	tmp.width(4);
	tmp << id.Data2 << "-";
	tmp.width(4);
	tmp << id.Data3 << "-";
	tmp.width(2);
	tmp << (int)id.Data4[0];
	tmp.width(2);
	tmp  << (int)id.Data4[1] << "-";
	tmp.width(2);
	tmp << (int)id.Data4[2];
	tmp.width(2);
	tmp  << (int)id.Data4[3];
	tmp.width(2);
	tmp  << (int)id.Data4[4];
	tmp.width(2);
	tmp  <<(int) id.Data4[5];
	tmp.width(2);
	tmp  << (int)id.Data4[6];
	tmp.width(2);
	tmp  << (int)id.Data4[7];
	string ret = tmp.str();
	return ret;
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
					succ->back()->settarget( (*i)->getid() );
					succ->back()->seti( -1 );
					succ = findjump( lab );
				}
			(*i)->erasefront();
			}
			++i;
		} else
			++i;
	}

	/*i = blocks.begin();

	while ( i != blocks.end() )
	{
		if ( (*i)->empty() )
		{
			for (int k = 0; k < (*i)->predecessors.size(); k++)
			{
				(*i)->predecessors[k]->successors.clear();
				(*i)->predpush_back((*i)->successors[0]);
			}
			(*i)->successors[0]->successors = (*i)->successors;
		}
		else
			++i;
	}*/

}

CInstructionsContainer* Function::findjump( int lab )
{
    for ( list<CInstructionsContainer*>::iterator i = blocks.begin(); i != blocks.end(); ++i)
    {
		if ( (*i)->empty() ) continue;
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

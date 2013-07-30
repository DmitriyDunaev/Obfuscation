#include "functions.h"

void Routine::cleanup()
{
	std::list<Function*>::iterator i = functs.begin();
    while ( i != functs.end() )
	{
		(*i)->cleanup();
		if ( (*i)->empty() )
		{
			std::cout << "Routine cleanup done" << std::endl;
			std::list<Function*>::iterator j = i;
			++i;
			delete *j;
			functs.erase(j);
		}
		else
			++i;
	}
}

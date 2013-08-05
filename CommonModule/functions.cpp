#include "functions.h"

void Routine::cleanup()
{
	std::list<Function*>::iterator i = functs.begin();
    while ( i != functs.end() )
	{
		(*i)->cleanup();
		if ( (*i)->empty() )
		{
			std::list<Function*>::iterator j = i;
			++i;
			delete *j;
			functs.erase(j);
		}
		else
			++i;
	}
}

void Routine::dump(stringstream& s)
{
    for ( std::list<Function*>::iterator i = functs.begin(); i != functs.end(); ++i )
	{
		s << "<Function GlobalID=\"" << (*i)->getname() <<  "\" CalledFrom=\"ExternalOnly\" ID=\"ID_";
		s << (*i)->getid();
		/*s << "\" ExternalLabel=\"";
		s << (*i)->getname();*/
		s << "\" ";
        (*i)->dump(s);
		s << "</Function>\n";
	}
}
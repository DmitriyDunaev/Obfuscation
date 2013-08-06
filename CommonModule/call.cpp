#include "call.h"

void Call::print(stringstream& s)
{
    s << "<Instruction PolyRequired=\"false\" ID=\"ID_";
	s << getid() << "\" StatementType=\"";
	s << "Procedural\"";
	if (retr)
	{
		if (result->gettype() !=constant) s << " RefVars=\"ID_" << result->getid() << "\"";
		s << ">retrieve " << result->getname();

	}
	else if (!param)
	{
		s << ">call ";
		string tmp = str;
		tmp.erase( tmp.find( " " ) );
		if ( rtn->find( tmp ) == nullptr )	
			s << str;
		else 
		{
			s << "ID_" << rtn->find( tmp )->getid();
			string tmp2 = str;
			tmp2.erase( 0, tmp.size() );
			s << tmp2;
		}
	}
	else
	{
		if (result->gettype() !=constant) s << " RefVars=\"ID_" << result->getid() <<"\"";
		s << ">param " << result->getname();
	}
    s << "</Instruction>"<< std::endl;
}
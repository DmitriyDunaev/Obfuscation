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
		s << str;

	}
	else
	{
		if (result->gettype() !=constant) s << " RefVars=\"ID_" << result->getid() <<"\"";
		s << ">param " << result->getname();
	}
    s << "</Instruction>"<< std::endl;
}
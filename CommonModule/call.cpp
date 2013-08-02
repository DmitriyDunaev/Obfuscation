#include "call.h"

void Call::print(stringstream& s)
{
    s << "<Instruction PolyRequired=\"false\" ID=\"ID_";
	s << getid() << "\" StatementType=\"";
	s << "Procedural" << "\"";
	if (!param)
	{
		s << ">";
		if (result != nullptr) s << result->getname() << " = ";
		s << str;
	}
	else
	{
		s << " RefVars=\"ID_" << result->getid() << "\">";
		s << "param " << result->getname();
	}
    s << "</Instruction>"<< std::endl;
}
#include "call.h"

void Call::print(stringstream& s)
{
    s << "<Instruction PolyRequired=\"false\" ID=\"ID_";
	s << getid() << "\" StatementType=\"";
	s << "Call" << "\">";
	if (result != nullptr) s << result->getname() << " = ";
	s << str;
    s << "</Instruction>"<< std::endl;
}
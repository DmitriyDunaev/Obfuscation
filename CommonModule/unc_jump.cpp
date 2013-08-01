#include "unc_jump.h"

void Unc_jump::print(stringstream& s)
{
    s << "<Instruction PolyRequired=\"false\" ID=\"ID_";
	s << getid() << "\" StatementType=\"";
	s << "UnconditionalJump";
	if (ret != nullptr )
	{
		s << "\" RefVars=\"";
		if (ret->gettype() != constant) s << "ID_" << ret->getid();
	}
	s << "\" >";
	s << "goto " << target->getid();
    s << "</Instruction>"<< std::endl;
}
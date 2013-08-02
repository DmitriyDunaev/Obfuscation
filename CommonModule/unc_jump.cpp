#include "unc_jump.h"

void Unc_jump::print(stringstream& s)
{
    s << "<Instruction PolyRequired=\"false\" ID=\"ID_";
	s << getid() << "\" StatementType=\"";
	
	if (ret != nullptr )
	{
		s << "Procedural";
		s << "\" RefVars=\"";
		if (ret->gettype() != constant) s << "ID_" << ret->getid();
	}
	else
		s << "UnconditionalJump";
	s << "\" >";
	if ( ret == nullptr ) s << "goto ID_" << target->getid();
	else s << "return " << ret->getname();
    s << "</Instruction>"<< std::endl;
}
#include "unc_jump.h"

void Unc_jump::print(stringstream& s)
{
    s << "<Instruction PolyRequired=\"false\" ID=\"ID_";
	s << getid() << "\" StatementType=\"";
	
	if ( i > -3 )
	{
		if ( ret == nullptr) s << "UnconditionalJump" << "\" >" << "goto ID_" << target;
		else { s << "Procedural" << "\" RefVars=\"";
		if (ret->gettype() != constant) s << "ID_" << ret->getid();
		s << "\">" << "return";
		s << " " << ret->getname(); }
	} else 
	{
		s << "Procedural";
		s << "\">" << "return";
	}
	s << "</Instruction>"<< std::endl;
}
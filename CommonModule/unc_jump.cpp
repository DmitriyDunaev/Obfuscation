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
		s << "\">";
	}
	if ( i > -3 )
	{
		s << "UnconditionalJump";
		s << "\" >";
		if ( ret == nullptr ) s << "goto ID_" << target->getid();
		else { s << "return";
		s << " " << ret->getname(); }
	} else 
	{
		s << "Procedural";
		s << "\">" << "return";
	}
	s << "</Instruction>"<< std::endl;
}
#include "cond_jump.h"

void Cond_jump::print(stringstream& s)
{
    s << "<Instruction PolyRequired=\"false\" ID=\"ID_";
	s << getid() << "\" StatementType=\"";
	s << "ConditionalJump\">";
	s << "if " << cond << " goto " << target->getid();
    s << "</Instruction>"<< std::endl;
}
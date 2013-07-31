#include "fakeexitblock.h"

void FakeExitBlock::print(stringstream &s)
{
    s << "<Instruction PolyRequired=\"false\" ID=\"ID_";
	s << getid() << "\" StatementType=\"";
	s << "Unknown" << "\">";
	s << "Return";
    s << "</Instruction>"<< std::endl;
}
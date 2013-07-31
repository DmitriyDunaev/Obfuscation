#include "fakeexitblock.h"

void FakeExitBlock::print(stringstream &s)
{
    s << "<Instruction PolyRequired=\"false\" ID=\"ID_";
	s << getid() << "\" StatementType=\"";
	s << "Unknown" << "\">";
	s << "Fakeexit";
    s << "</Instruction>"<< std::endl;
}
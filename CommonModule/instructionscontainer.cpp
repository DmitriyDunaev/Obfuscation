#include "instructionscontainer.h"

using namespace std;

string CInstructionsContainer::getid()	
{
	stringstream tmp;
	tmp.flags( stringstream::hex | stringstream::uppercase);
	tmp.fill('0');
	tmp.width(8);
	tmp << id.Data1 << "-";
	tmp.width(4);
	tmp << id.Data2 << "-";
	tmp.width(4);
	tmp << id.Data3 << "-";
	tmp.width(2);
	tmp << (int)id.Data4[0];
	tmp.width(2);
	tmp  << (int)id.Data4[1] << "-";
	tmp.width(2);
	tmp << (int)id.Data4[2];
	tmp.width(2);
	tmp  << (int)id.Data4[3];
	tmp.width(2);
	tmp  << (int)id.Data4[4];
	tmp.width(2);
	tmp  <<(int) id.Data4[5];
	tmp.width(2);
	tmp  << (int)id.Data4[6];
	tmp.width(2);
	tmp  << (int)id.Data4[7];
	string ret = tmp.str();
	return ret;
}

void CInstructionsContainer::push_back (CThreeAdressInstruction* inst)
{
    InstructionList.push_back(inst);
}

void CInstructionsContainer::clear ()
{
    for (list<CThreeAdressInstruction*>::iterator i = InstructionList.begin();
            i != InstructionList.end(); ++i)
        delete *i;
    InstructionList.clear();
}

CInstructionsContainer::~CInstructionsContainer()
{
    clear();
}

#include "instructionscontainer.h"

using namespace std;



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

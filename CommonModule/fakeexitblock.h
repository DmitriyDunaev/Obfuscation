#ifndef FAKEEXITBLOCK_H
#define FAKEEXITBLOCK_H

#include "threeadressinstruction.h"

using namespace std;

class FakeExitBlock : public CThreeAdressInstruction
{
public:
    FakeExitBlock(){}
    bool isexit() { return true;}
    void print(stringstream& s)
    {
        s  << "		<Instruction Type=\"original\" Label=\"\">return</Instruction><!-- EXIT -->" << endl;
    }
};

#endif // FAKEEXITBLOCK_H

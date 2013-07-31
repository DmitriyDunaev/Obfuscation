#ifndef FAKEEXITBLOCK_H
#define FAKEEXITBLOCK_H

#include "threeadressinstruction.h"

using namespace std;

class FakeExitBlock : public CThreeAdressInstruction
{
public:
    FakeExitBlock(){}
    bool isexit() { return true;}
    void print(stringstream& s);
};

#endif // FAKEEXITBLOCK_H

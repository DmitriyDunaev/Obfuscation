#ifndef FAKEEXITBLOCK_H
#define FAKEEXITBLOCK_H

#include "threeadressinstruction.h"

using namespace std;

class FakeExitBlock : public CThreeAdressInstruction
{
public:
    FakeExitBlock(){ UuidCreate( &id ); }
    bool isexit() { return true;}
	int geti() { return -2; }
    void print(stringstream& s);
};

#endif // FAKEEXITBLOCK_H

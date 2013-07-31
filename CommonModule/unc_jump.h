#ifndef UNC_JUMP_H
#define UNC_JUMP_H

#include "threeadressinstruction.h"

using namespace std;

class Unc_jump : public CThreeAdressInstruction
{
    string label;
    int i;
	CThreeAdressInstruction* target;
public:
    Unc_jump(string s, int i):label(s), i(i), target(this) {}
    string gets() { return label; }
    int geti() { return i; }
	void seti ( int in ) { i = in; }
    bool isuncjmp() { return true; }
    void print(stringstream& s);
	
	CThreeAdressInstruction* gettarget() {return target;}
	void settarget( CThreeAdressInstruction* t ) { target = t; }
};

#endif // UNC_JUMP_H

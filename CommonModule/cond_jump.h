#ifndef COND_JUMP_H
#define COND_JUMP_H

#include "threeadressinstruction.h"

using namespace std;

class Cond_jump : public CThreeAdressInstruction
{
    string cond;
    int i;
	CThreeAdressInstruction* target;
public:
    Cond_jump(string s, int i):cond(s), i(i), target(this) {}
    string gets() { return cond; }
    int geti() { return i; }
	void seti( int in ) { i = in; }
    void print(stringstream& s);

	CThreeAdressInstruction* gettarget() {return target;}
	void settarget( CThreeAdressInstruction* t ) { target = t; }
};

#endif // UNC_JUMP_H

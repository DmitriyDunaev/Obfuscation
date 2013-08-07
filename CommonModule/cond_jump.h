#ifndef COND_JUMP_H
#define COND_JUMP_H

#include "threeadressinstruction.h"
#include "variables.h"

using namespace std;

class Cond_jump : public CThreeAdressInstruction
{
    string op;
	COperand* ops[2];
	int opnum;
    int i;
	string target;
public:
    Cond_jump(string s, int i, CVariables* vars);
    string gets() { return op; }
    int geti() { return i; }
	void seti( int in ) { i = in; }
    void print(stringstream& s);

	string gettarget() {return target;}
	void settarget( string t ) { target = t; }
};

#endif // UNC_JUMP_H

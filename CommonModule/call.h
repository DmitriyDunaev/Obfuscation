#ifndef CALL_H
#define CALL_H

#include "threeadressinstruction.h"
#include <string>

using namespace std;

class Call : public CThreeAdressInstruction
{
    string str;
	COperand* result;
	bool param;
public:
    Call(string s):str(s), param(false) { result=nullptr; }
	Call(string s, COperand* res, bool p = false):str(s), result(res), param(p) {}

    string gets() { return str; }
    void print(stringstream& s);
};

#endif // CALL_H

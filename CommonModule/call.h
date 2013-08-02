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
	bool retr;
public:
    Call(string s):str(s), param(false), retr(false) { result=nullptr; }
	Call(string s, COperand* res, bool p = false, bool r = false):str(s), result(res), param(p), retr(r) {}

    string gets() { return str; }
    void print(stringstream& s);
};

#endif // CALL_H

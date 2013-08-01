#ifndef CALL_H
#define CALL_H

#include "threeadressinstruction.h"
#include <string>

using namespace std;

class Call : public CThreeAdressInstruction
{
    string str;
	COperand* result;
public:
    Call(string s):str(s){ result=nullptr; }
	Call(string s, COperand* res):str(s), result(res){}

    string gets() { return str; }
    void print(stringstream& s);
};

#endif // CALL_H

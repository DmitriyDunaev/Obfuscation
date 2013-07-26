#ifndef CALL_H
#define CALL_H

#include "threeadressinstruction.h"
#include <string>

using namespace std;

class Call : public CThreeAdressInstruction
{
    string s;
public:
    Call(string s):s(s){}
    string gets() { return s; }
    void print(stringstream& str)
    {
#ifdef DEBUG
        str  << "		<Instruction Type=\"original\" Label=\"" << s << "<\">/Instruction><!-- CALL -->" << endl;
#endif
    }
};

#endif // CALL_H

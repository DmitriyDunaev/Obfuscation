#ifndef COND_JUMP_H
#define COND_JUMP_H

#include "threeadressinstruction.h"

using namespace std;

class Cond_jump : public CThreeAdressInstruction
{
    string cond;
    int i;
public:
    Cond_jump(string s, int i):cond(s), i(i) {}
    string gets() { return cond; }
    int geti() { return i; }
    void print(stringstream& s)
    {
        s  << "		<Instruction Type=\"original\" Label=\"\">if " << cond << " goto LABEL_" << i << "</Instruction><!-- COND_JMP -->" << endl;
    }
};

#endif // UNC_JUMP_H

#ifndef UNC_JUMP_H
#define UNC_JUMP_H

#include "threeadressinstruction.h"

using namespace std;

class Unc_jump : public CThreeAdressInstruction
{
    string label;
    int i;
public:
    Unc_jump(string s, int i):label(s), i(i){}
    string gets() { return label; }
    int geti() { return i; }
    bool isuncjmp() { return true; }
    void print(ofstream& s)
    {
        s  << "		<Instruction Type=\"original\" Label=\"\">goto " << label << i << "</Instruction><!-- UNC_JMP -->" << endl;
    }
};

#endif // UNC_JUMP_H

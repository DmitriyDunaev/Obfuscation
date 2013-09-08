#ifndef LABEL_H
#define LABEL_H

#include <string>

#include "threeadressinstruction.h"

using namespace std;

class Label : public CThreeAdressInstruction
{
    string label;
    int i;
public:
    Label(string s, int i):label(s), i(i) {}
    string gets() { return label; }
    int geti() { return i; }
    bool islabel() {return true;}
    void print(stringstream& s)
    {
        //s  << "		<Instruction Type=\"original\" Label=\"" << label << i << "\"></Instruction><!-- LABEL -->" << endl;
    }
};

#endif // LABEL_H

#ifndef CTHREEADRESSINSTRUCTION_H
#define CTHREEADRESSINSTRUCTION_H

#ifdef DEBUG
    #include <iostream>
#endif

#include <string>
#include <iostream>
#include <fstream>

using namespace std;
#include "operand.h"

class CThreeAdressInstruction
{
public:

    CThreeAdressInstruction() {}
    virtual ~CThreeAdressInstruction() {}
    virtual int geti() { return -1; }
    virtual bool islabel() { return false; }
    virtual bool isuncjmp() { return false; }
    virtual bool isexit() {return false;}
    virtual void print(stringstream& s) {}

};

#endif // CTHREEADRESSINSTRUCTION_H

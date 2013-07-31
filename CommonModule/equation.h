#ifndef EQUATION_H
#define EQUATION_H

#include "threeadressinstruction.h"


class Equation : public CThreeAdressInstruction
{
    COperand* ops[2];
    bool neg;

public:
    Equation (COperand**, bool neg = false);
    virtual ~Equation() {}

    void print(stringstream& s);

};

#endif // EQUATION_H

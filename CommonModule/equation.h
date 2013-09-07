#ifndef EQUATION_H
#define EQUATION_H

#include "threeadressinstruction.h"


class Equation : public CThreeAdressInstruction
{
    COperand* ops[2];
    bool neg;
	bool adress;

public:
    Equation (COperand**, bool neg = false, bool adress = false);
    virtual ~Equation() {}

    void print(stringstream& s);

};

#endif // EQUATION_H

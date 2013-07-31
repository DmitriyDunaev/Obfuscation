#ifndef QUADRUPLE_H
#define QUADRUPLE_H

#include "threeadressinstruction.h"


class Quadruple : public CThreeAdressInstruction
{
    std::string oprtr;
    COperand* ops[3];

public:
    Quadruple (std::string, COperand**);
    virtual ~Quadruple() {}

    void print(stringstream& s);

};

#endif // QUADRUPLE_H

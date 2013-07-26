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

    void print(stringstream& s)
    {
         s << "		<Instruction Type=\"original\" Label=\"\">" << ops[0]->getname() << " := ";
         if (neg) s << "-";
         s << ops[1]->getname();
         s << "</Instruction><!-- EQUATION -->"<< std::endl;
    }

};

#endif // EQUATION_H

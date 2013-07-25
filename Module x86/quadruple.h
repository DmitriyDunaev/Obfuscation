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

    void print(ofstream& s)
    {
        s << "		<Instruction Type=\"original\" Label=\"\">" << ops[0]->getname() << " := " << ops[1]->getname();
        s << " " << oprtr << " " << ops[2]->getname();
        s << "</Instruction><!-- QUAD -->" << std::endl;
    }

};

#endif // QUADRUPLE_H

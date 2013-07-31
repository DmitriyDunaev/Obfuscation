#include "equation.h"


Equation::Equation (COperand** arr, bool neg) : neg(neg)
{
    for (int i = 0; i < 2; i++)
        ops[i] = arr[i];
}

void Equation::print(stringstream& s)
{
    s << "<Instruction PolyRequired=\"false\" ID=\"ID_";
	s << getid() << "\" StatementType=\"";
	if (neg) s << "UnaryAssignment"; else s << "Copy";
	s << "\" RefVars=\"ID_" << ops[0]->getid() << " ID_" << ops[1]->getid() << "\">";
	s << ops[0]->getname() << " := ";
    if (neg) s << "-";
    s << ops[1]->getname();
    s << "</Instruction>"<< std::endl;
}
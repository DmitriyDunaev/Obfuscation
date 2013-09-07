#include "equation.h"


Equation::Equation (COperand** arr, bool neg, bool adress) : neg(neg), adress(adress)
{
    for (int i = 0; i < 2; i++)
        ops[i] = arr[i];
}

void Equation::print(stringstream& s)
{
    s << "<Instruction PolyRequired=\"false\" ID=\"ID_";
	s << getid() << "\" StatementType=\"";
	if ( ops[0]->getp() || ops[1]->getp() )
	{
		s << "PointerAssignment";
	} else
	{
		if (neg) 
			s << "UnaryAssignment"; 
		else s << "Copy";
	}
	s << "\" RefVars=\"";
	if (ops[0]->gettype() != constant) s << "ID_" << ops[0]->getid();
	if (ops[1]->gettype() != constant) s << " ID_" << ops[1]->getid();
	s << "\">";
	if (ops[0]->getp() && !ops[1]->getp() && !adress) s << "* ";
	s << ops[0]->getname() << " := ";
    if (neg) s << "-";
	if (ops[0]->getp() && !ops[1]->getp() && adress) s << "&amp; ";
	if (ops[1]->getp() && !ops[0]->getp()) s << "&amp; ";
    s << ops[1]->getname();
    s << "</Instruction>"<< std::endl;
}
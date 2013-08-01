#include "quadruple.h"

using namespace std;

Quadruple::Quadruple(string op, COperand** arr)
{
    oprtr = op;
    for (int i = 0; i < 3; i++)
        ops[i] = arr[i];
}

void Quadruple::print(stringstream& s)
{
    s << "<Instruction PolyRequired=\"false\" ID=\"ID_";
	s << getid() << "\" StatementType=\"";
	s << "FullAssignment\" RefVars=\"";
	if (ops[0]->gettype() != constant) s << "ID_" << ops[0]->getid();
	if (ops[1]->gettype() != constant) s << " ID_" << ops[1]->getid();
	if (ops[2]->gettype() != constant) s << " ID_" << ops[2]->getid();
	s << "\">";
	s << ops[0]->getname() << " := ";
    s << ops[1]->getname() << " " << oprtr << " " << ops[2]->getname();
    s << "</Instruction>"<< std::endl;
}
#include "cond_jump.h"

void Cond_jump::print(stringstream& s)
{
    s << "<Instruction PolyRequired=\"false\" ID=\"ID_";
	s << getid() << "\" StatementType=\"";
	s << "ConditionalJump\" RefVars=\"";
	if (ops[0]->gettype() != constant) s << "ID_" << ops[0]->getid();
	if (opnum > 1)
	{
		if (ops[1]->gettype() != constant) s << " ID_" << ops[1]->getid();
	}
	s << "\">if " << ops[0]->getname();
	if ( opnum > 1 )
	{
		s << " ";
		if (op == "==")
			s << "==";
		else if (op == ">")
			s << "&gt;";
		else if (op == "<")
			s << "&lt;";
		else if (op == ">=")
			s << "&gt;=";
		else if (op == "<=")
			s << "&lt;=";
		s << " " << ops[1]->getname();
	}
	s << " goto ID_" << target;
    s << "</Instruction>"<< std::endl;
}

 Cond_jump::Cond_jump(std::string s, int i, CVariables* vars): i(i)
 {
	 if (((s.find("=")!= string::npos) || (s.find(">")!= string::npos) || (s.find("<")!= string::npos)))
	 {
		 opnum=2;
		 stringstream tmp(s);
		 string tmp1, tmp2, tmp3;
		 getline( tmp, tmp1, ' ' );
		 getline( tmp, tmp2, ' ' );
		 getline( tmp, tmp3);
		 op=tmp2;
		 ops[0]= (*vars)[tmp1];
		 ops[1]= (*vars)[tmp3];
	 }
	 else
	 {
		 opnum = 1;
		 ops[0]= (*vars)[s];
	 }
 }
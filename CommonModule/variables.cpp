#include "variables.h"

using namespace std;

COperand*& CVariables::operator[] (string s)
{
	if ( s[0] == '*') s.erase( 0, 1 );
    if ( s[0] == '&') s.erase( 0, 1 );
    if (!variables.count(s))
        variables[s] = new COperand(s);
    return variables[s];
}

void CVariables::clear ()
{
    for (map<string, COperand*>::iterator i = variables.begin(); i != variables.end(); ++i)
        delete i->second;
    variables.clear();
}

CVariables::~CVariables()
{
    clear();
}

void CVariables::dump(stringstream &s)
{
    stringstream str;
    for (std::map<std::string, COperand*>::iterator i = variables.begin(); i != variables.end(); ++i)
    {
		if ( ((i->second)->getuse() == input) && (!i->second->gettype() == constant))
		{
			str << "ID_" << ((*i).second)->getid() << " ";
		}
	}
	string tmp = str.str();
	if ( !tmp.empty() )
	{
		tmp.erase( tmp.size() -1);
		s << "RefInputVars=\"" << tmp << "\" ";
	}
	stringstream str2;
    for (std::map<std::string, COperand*>::iterator i = variables.begin(); i != variables.end(); ++i)
    {
		if ( ((i->second)->getuse() == output) && (!i->second->gettype() == constant))
		{
			str2 << "ID_" << ((*i).second)->getid() << " ";
		}
	}
	tmp = str2.str();
	if ( !tmp.empty() )
	{
		tmp.erase( tmp.size() -1);
		s << "RefOutputVars=\"" << tmp << "\">\n";
	}
	s << "	<Local>\n";
    for (std::map<std::string, COperand*>::iterator i = variables.begin(); i != variables.end(); ++i)
    {
		if ( /*((i->second)->getuse() == local) && */!(i->second->gettype() == constant) )
		{
			s << "		<Variable ID=\"ID_" << ((*i).second)->getid() << "\" ";
			string t = i->second->gett();
			if ( !t.empty())
			{
				if ( t == "char" ||  t == "signed char" ||  t == "unsigned char" )
					s << "Size=\"byte\" ";
				else if ( t == "int" ||  t == "signed int" ||  t == "unsigned int" )
					s << "Size=\"dword\" ";
				else if ( t == "short" ||  t == "signed short" ||  t == "unsigned short" )
					s << "Size=\"word\" ";
				else if ( t == "long" ||  t == "signed long" ||  t == "unsigned long" )
					s << "Size=\"qword\" ";
				
			} else s << "Size=\"dword\" ";
			s << "Pointer=\"";
			if ( i->second->getp() ) s << "true\"";
			else s << "false\"";
			s << " GlobalID=\"" << i->second->getoriginalname() << "\"";
			s << ">";
			((*i).second)->print(s);
			s << "</Variable>\n";
		}
	}

	s << "	</Local>\n";

}
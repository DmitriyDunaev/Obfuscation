#include "variables.h"

using namespace std;

COperand*& CVariables::operator[] (string s)
{
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
    s << "	<Inputs>\n		 <Original>\n";
    for (std::map<std::string, COperand*>::iterator i = variables.begin(); i != variables.end(); ++i)
    {
		if ( (i->second)->getuse() == 0 )
		{
			s << "				<Variable ID=\"ID_" << ((*i).second)->getid() << "\" ";
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
				
			} else s << "Size=\"unknown\" ";
			s << "Pointer=\"false\">";
			((*i).second)->print(s);
			s << "</Variable>\n";
		}
	}

	s << "		</Original>\n	</Inputs>\n";
	s << "	<Outputs>\n		 <Original>\n";
    for (std::map<std::string, COperand*>::iterator i = variables.begin(); i != variables.end(); ++i)
    {
		if ( (i->second)->getuse() == 1 )
		{
			s << "				<Variable ID=\"ID_" << ((*i).second)->getid() << "\" ";
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
				
			} else s << "Size=\"unknown\" ";
			s << "Pointer=\"false\">";
			((*i).second)->print(s);
			s << "</Variable>\n";
		}
	}

	s << "		</Original>\n	</Outputs>\n";
	s << "	<Locals>\n		 <Original>\n";
    for (std::map<std::string, COperand*>::iterator i = variables.begin(); i != variables.end(); ++i)
    {
		if ( (i->second)->getuse() == 2 )
		{
			s << "				<Variable ID=\"ID_" << ((*i).second)->getid() << "\" ";
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
				
			} else s << "Size=\"unknown\" ";
			s << "Pointer=\"";
			if ( i->second->getp() ) s << "true\">";
			else s << "false\">";
			((*i).second)->print(s);
			s << "</Variable>\n";
		}
	}

	s << "		</Original>\n	</Locals>\n";

}
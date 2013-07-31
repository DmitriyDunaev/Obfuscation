#ifndef COPERAND_H
#define COPERAND_H

#define DEBUG

#ifdef DEBUG
    #include <iostream>
#endif

#include <string>
#include <sstream>
#include <iomanip>

#include "labelgenerator.h"

enum OpType
{
    constant,
    variable,
    temp_var
};

class COperand
{
    std::string name;
    OpType type;

    std::string t;

	UUID id;
public:
    COperand(std::string s);

    void print(std::stringstream &s);
    std::string getname();
    std::string gett() {return t; }
    void sett(std::string s) { t=s; }
	
	std::string getid()
	{
		std::stringstream tmp;
		tmp.flags( std::stringstream::hex );
		tmp << id.Data1 << "-" << id.Data2 << "-" << id.Data3 << "-" << (int)id.Data4[0] << (int)id.Data4[1] << "-";
		tmp.width(2);
		tmp << std::setfill('0');
		tmp << (int)id.Data4[2] << (int)id.Data4[3] << (int)id.Data4[4] <<(int) id.Data4[5] << (int)id.Data4[6] << (int)id.Data4[7];
		std::string ret = tmp.str();
		return ret;
	}
};

#endif // COPERAND_H

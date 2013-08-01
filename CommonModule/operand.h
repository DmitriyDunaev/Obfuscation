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

enum UseType
{
	input,
	output,
	local
};

class COperand
{
    std::string name;
    OpType type;
	UseType usetype;
    std::string t;
	bool p;
	UUID id;
public:
    COperand(std::string s);

    void print(std::stringstream &s);
    std::string getname();
	bool getp() { return p; }
	void setp(bool i) { 
		p=i; 
	}
	OpType gettype()
	{
		return type;
	}
    std::string gett() {return t; }
    void sett(std::string s) { t=s; }
	void setuse( UseType i ) {
		usetype = i; 
	}
	UseType getuse () { return usetype; }
	
	std::string getid();
};

#endif // COPERAND_H

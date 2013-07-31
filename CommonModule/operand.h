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
	int usetype;
    std::string t;

	UUID id;
public:
    COperand(std::string s);

    void print(std::stringstream &s);
    std::string getname();
    std::string gett() {return t; }
    void sett(std::string s) { t=s; }
	void setuse( int i ) {
		usetype = i; 
	}
	int getuse () { return usetype; }
	
	std::string getid();
};

#endif // COPERAND_H

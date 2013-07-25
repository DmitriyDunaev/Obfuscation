#ifndef COPERAND_H
#define COPERAND_H

#define DEBUG

#ifdef DEBUG
    #include <iostream>
#endif

#include <string>
#include <sstream>

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

public:
    COperand(std::string s);

#ifdef DEBUG
    void print();
#endif
    std::string getname();
    std::string gett() {return t; }
    void sett(std::string s) { t=s; }
};

#endif // COPERAND_H

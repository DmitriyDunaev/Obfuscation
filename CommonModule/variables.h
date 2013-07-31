#ifndef CVARIABLES_H
#define CVARIABLES_H

//#define DEBUG

#include <map>
#include <string>

#include "operand.h"

class CVariables
{
    std::map<std::string, COperand*> variables;

public:
    CVariables() {}
    ~CVariables();
    void clear();
    COperand*& operator[] (std::string s);

#ifdef DEBUG
    void dump(std::stringstream &s);
#endif
};

#endif // CVARIABLES_H

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
    void dump()
    {
        std::cout << std::endl << "----------- Variables -----------\n\n";
        for (std::map<std::string, COperand*>::iterator i = variables.begin();
                                                        i != variables.end(); ++i)
        ((*i).second)->print();
    }
#endif
};

#endif // CVARIABLES_H

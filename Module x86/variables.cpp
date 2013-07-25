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

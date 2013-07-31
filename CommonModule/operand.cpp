#include "operand.h"

using namespace std;

COperand::COperand(std::string s)
{
	UuidCreate( &id );
    name = s;
    t = "\0";
    type = variable;

    /* temporary variables are like: t_xx */
    if (name[0] == 't' && name[1] == '_')
        type = temp_var;

    /* constants contain numbers only */
    unsigned int i;
    for (i = 0; name[i] >= '0' && name[i] <= '9'; i++);
    if (i == name.size())
        type = constant;

    /* if we got here, than it's really a variable*/
}

std::string COperand::getname()
{
    return name;
}

#ifdef DEBUG
void COperand::print (stringstream &s)
{
    s << name;
    /*if (type == constant)
        cout << "constant";
    if (type == temp_var)
        cout << "temporary variable";
    if (type == variable)
        cout << "variable";

    if (t != "\0" )
        cout << " " << t;

    cout << endl;*/
}
#endif

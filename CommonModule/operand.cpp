#include "operand.h"

using namespace std;

COperand::COperand(std::string s)
{
	UuidCreate( &id );
    name = s;
    type = variable;
	usetype = local;
	p = false;

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

std::string COperand::getid()
{
	stringstream tmp;
	tmp.flags( stringstream::hex | stringstream::uppercase);
	tmp.fill('0');
	tmp.width(8);
	tmp << id.Data1 << "-";
	tmp.width(4);
	tmp << id.Data2 << "-";
	tmp.width(4);
	tmp << id.Data3 << "-";
	tmp.width(2);
	tmp << (int)id.Data4[0];
	tmp.width(2);
	tmp  << (int)id.Data4[1] << "-";
	tmp.width(2);
	tmp << (int)id.Data4[2];
	tmp.width(2);
	tmp  << (int)id.Data4[3];
	tmp.width(2);
	tmp  << (int)id.Data4[4];
	tmp.width(2);
	tmp  <<(int) id.Data4[5];
	tmp.width(2);
	tmp  << (int)id.Data4[6];
	tmp.width(2);
	tmp  << (int)id.Data4[7];
	string ret = tmp.str();
	return ret;
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

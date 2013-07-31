#ifndef CTHREEADRESSINSTRUCTION_H
#define CTHREEADRESSINSTRUCTION_H

#ifdef DEBUG
    #include <iostream>
#endif

#include <string>
#include <iostream>
#include <fstream>

#include "operand.h"
#include "labelgenerator.h"

class CThreeAdressInstruction
{
protected:
	UUID id;
public:

    CThreeAdressInstruction() 
	{
		UuidCreate( &id );
	}
    virtual ~CThreeAdressInstruction() {}
    virtual int geti() { return -1; }
    virtual bool islabel() { return false; }
    virtual bool isuncjmp() { return false; }
    virtual bool isexit() {return false;}
    virtual void print(stringstream& s) {}
	virtual void settarget( CThreeAdressInstruction* t ) {}
	virtual void seti(int i) {}

	string getid()
	{
		stringstream tmp;
		tmp.flags( stringstream::hex );
		tmp << id.Data1 << "-" << id.Data2 << "-" << id.Data3 << "-" << (int)id.Data4[0] << (int)id.Data4[1] << "-";
		tmp.width(2);
		tmp << std::setfill('0');
		tmp.flags();
		tmp << (int)id.Data4[2] << (int)id.Data4[3] << (int)id.Data4[4] <<(int) id.Data4[5] << (int)id.Data4[6] << (int)id.Data4[7];
		string ret = tmp.str();
		return ret;
	}

};

#endif // CTHREEADRESSINSTRUCTION_H

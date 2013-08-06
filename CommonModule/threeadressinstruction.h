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
	virtual std::string gets() { return ""; }
	
	string getid();

};

#endif // CTHREEADRESSINSTRUCTION_H

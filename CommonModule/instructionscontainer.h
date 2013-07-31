#ifndef CINSTRUCTIONCONTAINER_H
#define CINSTRUCTIONCONTAINER_H

#define DEBUG

    #include <iostream>
#include <fstream>

#include <list>
#include <vector>

#include "threeadressinstruction.h"
#include "variables.h"
#include "labelgenerator.h"

using namespace std;

class CInstructionsContainer
{
    UUID id;

    std::list<CThreeAdressInstruction*> InstructionList;
    std::vector<CInstructionsContainer*> predecessors;

    std::vector<CInstructionsContainer*> successors;

public:
    CInstructionsContainer(UUID i) : id(i) {}
    ~CInstructionsContainer();
    void clear ();
    void push_back (CThreeAdressInstruction*);

    CThreeAdressInstruction* front() { return InstructionList.front(); }
	CThreeAdressInstruction* frontplus()
	{
		std::list<CThreeAdressInstruction*>::iterator i = InstructionList.begin();
		++i;
		return *i;
	}
	void erasefront()
	{
		InstructionList.erase( InstructionList.begin() );
	}

    bool empty()
    {
        return InstructionList.empty();
    }
    CThreeAdressInstruction* back() { return InstructionList.back(); }

	void succpush_back(CInstructionsContainer* c) { successors.push_back(c); }
	void predpush_back(CInstructionsContainer* c) { predecessors.push_back(c); }

    string getid()
	{
		stringstream tmp;
		tmp.flags( stringstream::hex );
		tmp << id.Data1 << "-" << id.Data2 << "-" << id.Data3 << "-" << (int)id.Data4[0] << (int)id.Data4[1] << "-";
		tmp.width(2);
		tmp.flags();
		tmp << std::setfill('0');
		tmp << (int)id.Data4[2] << (int)id.Data4[3] << (int)id.Data4[4] <<(int) id.Data4[5] << (int)id.Data4[6] << (int)id.Data4[7];
		string ret = tmp.str();
		return ret;
	}

#ifdef DEBUG
        void dump (stringstream& s)
        {

            s << "	<BasicBlock ";
			s << "ID=\"ID_" << getid() << "\" ";
            stringstream tmp;
            for (std::vector<CInstructionsContainer*>::iterator i = predecessors.begin();
                                            i != predecessors.end(); ++i)
                {
                    tmp << "ID_" << (*i)->getid();
                    std::vector<CInstructionsContainer*>::iterator j=i;
                    ++j;
                    if (j != predecessors.end()) tmp << " ";
                }
            string t;
            t = tmp.str();
            if ( t != "")
            {
                s << "Predecessors=\"" << t << "\" ";
            }
            stringstream tmp2;
            string t2;
            for (std::vector<CInstructionsContainer*>::iterator i = successors.begin();
                                            i != successors.end(); ++i)
                {
                    tmp2 << "ID_" << (*i)->getid();
                    std::vector<CInstructionsContainer*>::iterator j=i;
                    ++j;
                    if (j != successors.end()) tmp2 << " ";
                }
            t2 = tmp2.str();
            if ( t2 != "")
            {
                s << "Successors=\"" << t2 << "\"";
            }
			s << ">\n";
            for (std::list<CThreeAdressInstruction*>::iterator i = InstructionList.begin();
                                            i != InstructionList.end(); ++i)
                {
                    (*i)->print(s);
                }
            s << "	</BasicBlock>\n";

        }
#endif
};

#endif // CINSTRUCTIONCONTAINER_H

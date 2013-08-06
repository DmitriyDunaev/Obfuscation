#ifndef CINSTRUCTIONCONTAINER_H
#define CINSTRUCTIONCONTAINER_H

#define DEBUG

    #include <iostream>
#include <fstream>

#include <list>
#include <vector>
#include <iomanip>

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

    string getid();

	bool findcall( string s )
	{
		for ( list<CThreeAdressInstruction*>::iterator	i = InstructionList.begin(); i != InstructionList.end(); ++i)
		{
			if( (*i)->gets().find( s ) != string::npos ) 
				return true;
		}
		return false;
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
					s << "		";
                    (*i)->print(s);
                }
            s << "	</BasicBlock>\n";

        }
#endif
};

#endif // CINSTRUCTIONCONTAINER_H

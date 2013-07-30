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
    UUID block_id;

    std::list<CThreeAdressInstruction*> InstructionList;
    std::vector<CInstructionsContainer*> predecessors;

    std::vector<CInstructionsContainer*> successors;

public:
    CInstructionsContainer(UUID i) : block_id(i) {}
    ~CInstructionsContainer();
    void clear ();
    void push_back (CThreeAdressInstruction*);

    CThreeAdressInstruction* front() { return InstructionList.front(); }

    bool empty()
    {
        return InstructionList.empty();
    }
    CThreeAdressInstruction* back() { return InstructionList.back(); }

	void succpush_back(CInstructionsContainer* c) { successors.push_back(c); }
	void predpush_back(CInstructionsContainer* c) { predecessors.push_back(c); }

    UUID getblockid() { return block_id; }

#ifdef DEBUG
        void dump (stringstream& s)
        {

            s << "	<BasicBlock ";
            stringstream tmp;
            for (std::vector<CInstructionsContainer*>::iterator i = predecessors.begin();
                                            i != predecessors.end(); ++i)
                {
                    tmp << "ID_" << (*i)->getblockid().Data1;
                    std::vector<CInstructionsContainer*>::iterator j=i;
                    ++j;
                    if (j != predecessors.end()) tmp << " ";
                }
            string t;
            t = tmp.str();
            if ( t != "")
            {
                s << "In=\"" << t << "\" ";
            }
            stringstream tmp2;
            string t2;
            for (std::vector<CInstructionsContainer*>::iterator i = successors.begin();
                                            i != successors.end(); ++i)
                {
                    tmp2 << "ID_" << (*i)->getblockid().Data1;
                    std::vector<CInstructionsContainer*>::iterator j=i;
                    ++j;
                    if (j != successors.end()) tmp2 << " ";
                }
            t2 = tmp2.str();
            if ( t2 != "")
            {
                s << "Out=\"" << t2 << "\" ";
            }
			s << "ID=\"ID_" << block_id.Data1 << "\">\n";
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

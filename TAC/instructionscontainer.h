#ifndef CINSTRUCTIONCONTAINER_H
#define CINSTRUCTIONCONTAINER_H

#define DEBUG

    #include <iostream>
#include <fstream>

#include <list>
#include <vector>

#include "threeadressinstruction.h"
#include "variables.h"

using namespace std;

class CInstructionsContainer
{
    int block_id;

    std::list<CThreeAdressInstruction*> InstructionList;
    std::vector<CInstructionsContainer*> predecessors;

    std::vector<CInstructionsContainer*> successors;

public:
    CInstructionsContainer(int i) : block_id(i) {}
    ~CInstructionsContainer();
    void clear ();
    void push_back (CThreeAdressInstruction*);

    CThreeAdressInstruction* front() { return InstructionList.front(); }

    bool empty()
    {
        return InstructionList.empty();
    }
    CThreeAdressInstruction* back() { return InstructionList.back(); }

    friend class Assistant;
    friend class Blocks;

    int getblockid() { return block_id; }

#ifdef DEBUG
        void dump (ofstream& s)
        {

            s << "	<BasicBlock ";
            stringstream tmp;
            for (std::vector<CInstructionsContainer*>::iterator i = predecessors.begin();
                                            i != predecessors.end(); ++i)
                {
                    tmp << "ID_" << (*i)->getblockid();
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
            tmp;
            for (std::vector<CInstructionsContainer*>::iterator i = successors.begin();
                                            i != successors.end(); ++i)
                {
                    tmp << "ID_" << (*i)->getblockid();
                    std::vector<CInstructionsContainer*>::iterator j=i;
                    ++j;
                    if (j != successors.end()) tmp << " ";
                }
            t;
            t = tmp.str();
            if ( t != "")
            {
                s << "Out=\"" << t << "\" ";
            }
            s << "ID=\"ID_" << block_id << "\">\n";
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

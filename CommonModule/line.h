#ifndef LINE_H
#define LINE_H

#include "threeadressinstruction.h"

using namespace std;

class Line : public CThreeAdressInstruction
{
     string s;
public:
    Line(string s):s(s) {}
    string gets() { return s; }
    void repl(size_t f) { s.replace(f, 3, "=- "); }
    void ins(size_t f) { s.insert(f, " "); }
    void ersbeg() { s.erase(0, 1); }
    void ersmid(size_t from, int i) { s.erase( from, i); }
    void ersend() { s.erase(s.size()-1, 1); }
    void print(stringstream& str)
    {
        str << "		<Instruction Type=\"original\" Label=\"\">" << s << "</Instruction><!-- LINE -->" << endl;
    }

};

#endif // LINE_H

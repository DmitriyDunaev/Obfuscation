#ifndef LINE_H
#define LINE_H

#include "threeadressinstruction.h"

using namespace std;

class Line : public CThreeAdressInstruction
{
     string name;
public:
    Line(string s):name(s) {}
    string gets() { return name; }
    void repl(size_t f, bool address=false) 
	{ 
		if (address) 
			name.replace(f+1, 3, "=& "); 
		else 
			name.replace(f+1, 3, "=- "); 
	}
    void ins(size_t f) { name.insert(f, " "); }
    void ersbeg() { name.erase(0, 1); }
    void ersmid(size_t from, int i) { name.erase( from, i); }
    void ersend() { name.erase(name.size()-1, 1); }
    void print(stringstream& s);

};

#endif // LINE_H

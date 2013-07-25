#ifndef LABELGENERATOR_H
#define LABELGENERATOR_H

#include <string>
#include <sstream>

using namespace std;

class LabelGenerator
{
    int i;
public:
    LabelGenerator():i(0) {}
    string get()
    {
        i++;
        string tmp;
        stringstream szeretlek(tmp);
        szeretlek << "LABEL_" << i;
        tmp += szeretlek.str();
        return tmp;
    }
};

#endif // LABELGENERATOR_H

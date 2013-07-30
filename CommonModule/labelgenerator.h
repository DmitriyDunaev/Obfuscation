#ifndef LABELGENERATOR_H
#define LABELGENERATOR_H

#include <string>
#include <sstream>
#include <Rpc.h>
#include "objbase.h"
#include <iostream>
#pragma comment (lib, "Rpcrt4.lib")
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
	UUID getid() { UUID j; UuidCreate( &j ); return j; }
	GUID getguid()
	{ 
		::CoInitialize(0);
		GUID Guid = {0};
		::CoCreateGuid( &Guid );
		::CoUninitialize();
		return Guid;
	}
};

#endif // LABELGENERATOR_H

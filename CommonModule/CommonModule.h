// CommonModule.h
#pragma once
#include <msxml.h>
#include <iostream>
using namespace System;

namespace CommonModule {


	public enum class InputType
	{
		Assembler,
		PseudoCode
	};

	public enum class PlatformType
	{
		x86,
		x64
	};


	public ref class Reader
	{
	public:
		void DoStuff()
		{
			std::cout << "ASD" << std::endl;
		}
		string DoStuffNop();
	};

	public ref class InputProvider
	{
	public:
		Xml::XmlDocument^ Read (InputType it, PlatformType pt);
	private:
		//XMLDocument
	};

}

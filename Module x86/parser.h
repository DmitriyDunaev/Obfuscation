#ifndef PARSER_H
#define PARSER_H

#define DEBUG2

#ifdef DEBUG
    #include <iostream>
#endif

#include <string>
#include <stack>
#include <list>
#include <sstream>

#include "instructionscontainer.h"
#include "quadruple.h"
#include "equation.h"

enum TokenType
{
    var,  //variable
    num,  //number
    par,  //parenthese
    op,   //operator
    equ
};

class Parser
{
    std::stringstream is;           // the input stream
    std::list<std::string> tokens;  // list for the polish notation
    std::stack<std::string> ops;    // stack for the operator
    CInstructionsContainer& cont;
    std::string for_if;

    CVariables& vars;

    static unsigned int TempNum;    // to store the number of used temporary variables

    void pre_proc();                // deals with ++i and friends
    std::string inc_or_dec (std::string, bool);
    void parse();                   // reads the input stream and makes the appropriate tokens list
    void make_tac ();               // reads the tokens list and makes TAC

    static int prec (std::string);
    static TokenType type (std::string);

public:
    Parser(std::string s, CInstructionsContainer& c, CVariables& vars): is(s), cont(c), vars(vars) {};
    void work( bool need_to_preproc = true );
    std::string str() { return for_if; }

#ifdef DEBUG2
    void print ()
    {
        for (std::list<std::string>::iterator i = tokens.begin(); i != tokens.end(); ++i)
            std::cout << *i << " ";
        std::cout << std::endl;
    }
#endif
};

#endif // PARSER_H

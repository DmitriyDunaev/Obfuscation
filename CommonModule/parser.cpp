#include "parser.h"

//#define DEBUG2

using namespace std;

unsigned int Parser::TempNum = 0;

void Parser::work( bool need_to_preproc )
{
    if (need_to_preproc)
        pre_proc();
    else
    {
        parse();
        make_tac();
    }
}

void Parser::pre_proc()
{
    stringstream tmp1(is.str());
    stringstream tmp2;
    string token;

    do
    {
        is >> token;
        //cout << token << " -> ";
        if (type(token) ==  var)
            token = inc_or_dec(token, true);
        //cout << token << endl;
        tmp2 << " " << token;
    } while ( !is.eof() );
    is.str(tmp2.str());
    is.seekg (0, is.beg);

    work(false);

    do
    {
        tmp1 >> token;
        if (type(token) ==  var)
            token = inc_or_dec(token, false);
    } while ( !tmp1.eof() );
}

string Parser::inc_or_dec (string s, bool before)
{
    if (s.size() < 3)
        return s;

    string bgn = s.substr(0, 2);
    string nd = s.substr(s.size() - 2);

    if (bgn == "++" || nd == "++" || bgn == "--" || nd == "--")
    {
        if (bgn == "++" || bgn == "--")
        {
            s = s.substr(2);
            if (before)
            {
                stringstream ss;
                ss << s << " " << bgn[0] << "= 1";
                Parser tmp(ss.str(), cont, vars);
                tmp.work(false);
            }
        }
        else
        {
            s = s.substr(0, s.size() - 2);
            if (!before)
            {
                stringstream ss;
                ss << s << " " << nd[0] << "= 1";
                Parser tmp(ss.str(), cont, vars);
                tmp.work(false);
            }
        }
//        tokens.push_back(res);
//        tokens.push_back("1");
//        if (bgn == "++" || nd == "++")
//            tokens.push_back("+");
//        else
//            tokens.push_back("-");
//        return true;
    }
//    else
//        return false;
    return s;
}

void Parser::parse()
{
    string token;
    OwnTokenType typ;

    do
    {
        is >> token;
        typ = type(token);
        switch (typ)
        {
            case var:
            case num:
                tokens.push_back(token);
                break;

            case par:
                if (token == "(")
                    ops.push(token);
                else
                {
                    while (ops.top() != "(")
                    {
                        tokens.push_back (ops.top());
                        ops.pop();
                    }
                    ops.pop();
                }
                break;

            case op:
            case equ:
                while ( !ops.empty() && prec(token) <= prec(ops.top()) )
                {
                    tokens.push_back(ops.top());
                    ops.pop();
                }
                ops.push(token);
                break;
        }
    } while ( !is.eof() );
    while ( !ops.empty() )
    {
        tokens.push_back(ops.top());
        ops.pop();
    }
}

void Parser::make_tac()
{
    //CVariables &vars = cont.getVars();
    COperand* arr[3];
    OwnTokenType typ;
    list<string>::iterator i = tokens.begin();
    while (i != tokens.end())
    {
        typ = type(*i);
        if ( typ == op || typ == equ )
        {
            stringstream tmp;
            tmp << "t_" << Parser::TempNum;
            Parser::TempNum++;
            string tmp_var = tmp.str();

            list<string>::iterator k = --i;
            list<string>::iterator j = k--;     // j = i - 1, k = i - 2
            ++i;

            if (typ == equ)
            {
                arr[0] = vars[*k];
                arr[1] = vars[*j];
                if (*i == "+=" || *i == "-=" || *i == "*=" || *i == "/=")
                {
                    arr[2] = vars[*k];
                    cont.push_back (new Quadruple((*i).substr(0, 1), arr));
                }
                else if (*i == "=-")
                    cont.push_back (new Equation(arr, true));
				else if (*i == "=&")
                    cont.push_back (new Equation(arr, false, true));
                else if (*i == "=")
                    cont.push_back (new Equation(arr));
                else
                {
                    stringstream for_if_stream;
                    for_if_stream << *k << " " << *i << " " << *j;
                    for_if = for_if_stream.str();
                }
                list<string>::iterator tmp = i;
                ++i;
                tokens.erase(tmp);
            }
            else
            {
                arr[0] = vars[tmp_var];
                arr[1] = vars[*k];
                arr[2] = vars[*j];
                cont.push_back (new Quadruple(*i, arr));
                *i = tmp_var;
                ++i;
            }
            tokens.erase(j);
            tokens.erase(k);
        }
        else
            ++i;
    }
}

int Parser::prec (string token)
{
    /* +=, >=, ==, etc */
    if (token.size() > 1)
        return 1;

    char op = token[0];
    switch (op)
    {
        case '+':
        case '-':
        case '|':
            return 3;

        case '/':
        case '*':
        case '&':
            return 4;

        case '(':
            return 2;

        case '=':
        case '>':
        case '<':
            return 1;

        /* just to avoid the warning */
        default:
            return 0;
    }
}

OwnTokenType Parser::type (string token)
{
    /* check if it's a parenthese */
    if (token == "(" || token == ")")
    {
        #ifdef DEBUG2
            cout << "parenthese found: " << token << endl;
        #endif // DEBUG2
        return par;
    }

    /* some kind of equation*/
    if (token == "=" || token == "+=" || token == "-=" || token == "*=" || token == "/=" ||
        token == ">" || token == "<"  || token == "<=" || token == ">=" || token == "==" ||
        token == "=-"|| token == "=&")
        return equ;

    /* an operator */
    if (token == "+" || token == "-" || token == "*" || token == "/" || token == "|" || token == "&")
    {
        #ifdef DEBUG2
            cout << "operator found: " << token << endl;
        #endif // DEBUG2
        return op;
    }

    /* a number */
    unsigned int i;
    for (i = 0; token[i] >= '0' && token[i] <= '9'; i++);
    if (i == token.size())
    {
        #ifdef DEBUG2
            cout << "number found: " << token << endl;
        #endif // DEBUG2
        return num;
    }

    /* if we get here, then it's a variable */
    #ifdef DEBUG2
        cout << "variable found: " << token << endl;
    #endif // DEBUG2
    return var;
}

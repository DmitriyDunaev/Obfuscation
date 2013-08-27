#include "assistant.h"

#include <cstdlib>
#include <stdio.h>

void Assistant::parse(string* s)
{
	if (((s->find("=")!= string::npos) || (s->find(">")!= string::npos)
                  || (s->find("<")!= string::npos)) && (*s != "1"))
    {
        Parser p(*s, *cnt, rtn->back()->Vars);
        p.work();
        *s = p.str();
    }
}

void Assistant::getfor(string from, string *s1, string *s2, string *s3)
{
    stringstream ss( from.substr(from.find("(")+2, from.size()) );
    getline(ss, *s1, ';');
    getline(ss, *s2, ';');
    if (*s2 == " ") *s2 = "1";
    getline(ss, *s3, ')');
    s3->erase(s3->size()-1, 1);

    parse(s2);
}

void Assistant::getcond(string from, string *s1)
{
    string tmp = from.substr(from.find("(")+2, from.size());
    tmp.erase(tmp.size()-1, 1);
    tmp.erase(tmp.size()-1, 1);
    stringstream ss(tmp);
    getline(ss, *s1, ';');

    parse(s1);
}

void Assistant::newblock()
{
    UUID id = l.getid();
    rtn->back()->blocks.push_back(new CInstructionsContainer(id));
    cnt = rtn->back()->back();
}

void Assistant::generateif(string deal, string label)
{
    size_t found = deal.find("&&");
    size_t found2= deal.find("||");


    if (found == string::npos)
    {
        if (found2 == string::npos)
        {
            parse(&deal);
            string tmp = label;
            int i=0;
            tmp.erase(0, 6);
            stringstream ss(tmp);
            ss >> i;
            label.erase(6, label.size());
            cnt->push_back( new Cond_jump(deal, i, &rtn->back()->Vars) );

            newblock();
        }
        else
        {
            string IF, F, tmp;
            IF=l.get();
            F=l.get();

            string deal1, deal2;
            deal2 = deal.substr( found2, deal.size() );
            deal2.erase(0, 3);
            deal.erase( found2, deal.size() );
            deal1 = deal;
            deal1.erase(deal1.size()-1, 1);

            generateif(deal1, IF);
            generategoto(F);
            newblock();
            generatelabel(IF);
            generateif(deal2, label);
            newblock();
            generatelabel(F);
        }
    }
    else
    {
        string deal1, deal2;
        deal2 = deal.substr( found, deal.size() );
        deal2.erase(0, 3);
        deal.erase( found, deal.size() );
        deal1 = deal;
        deal1.erase(deal1.size()-1, 1);
        generateif(deal1, label);
        generateif(deal2, label);
    }
}

void Assistant::generategoto(string label)
{
    string tmp = label;
    int i=0;
    tmp.erase(0, 6);
    stringstream ss(tmp);
    ss >> i;
    label.erase(6, label.size());
    cnt->push_back( new Unc_jump(label, i));
    newblock();
}

void Assistant::generatelabel(string label)
{
    newblock();
    string tmp = label;
    int i=0;
    tmp.erase(0, 6);
    stringstream ss(tmp);
    ss >> i;
    label.erase(6, label.size());
    cnt->push_back( new Label(label, i));
}


void Assistant::preproc()
{
	/*list<Line>::iterator l = lines->begin();
	while ( l != lines->end())
	{
		while((l->gets())[0] == ' ') l->ersbeg();
		if (l->gets() == "\n")
		{
			list<Line>::iterator j;
			j = l;
			++l;
			lines->erase(j);
		} else ++l;
	}*/

    for (list<Line>::iterator i = lines->begin(); i != lines->end(); ++i)
    {
        if ( i->gets().find("//") != string::npos )
		{
            i->ersmid( i->gets().find("//"), i->gets().size() );
		}
		
    }

    for (list<Line>::iterator i = lines->begin(); i != lines->end(); ++i)
    {
        size_t found = i->gets().find("*(_DWORD *)");
        while (found != string::npos)
        {
            i->ersmid(found+1, 10);
            found = i->gets().find("*(_DWORD *)");
        }
		found = i->gets().find("*(_BYTE *)");
        while (found != string::npos)
        {
            i->ersmid(found+1, 9);
            found = i->gets().find("*(_BYTE *)");
        }
    }

    for (list<Line>::iterator i = lines->begin(); i != lines->end(); ++i)
    {
		while((i->gets()).c_str()[0] == ' ') i->ersbeg();
		while((i->gets()).c_str()[i->gets().size()-1] == ' ') i->ersend();

        if ( !i->gets().empty() )
		{
			size_t found = i->gets().find(" = -");
			if (found != string::npos)
			{
				i->repl(found);
			}
			if((i->gets())[i->gets().size()-1] == ';') i->ersend();
		}
    }

	list<Line>::iterator k = lines->begin();
	while ( k != lines->end())
	{
		if (k->gets()[0] == '\n')
		{
			list<Line>::iterator j;
			j = k;
			++k;
			lines->erase(j);
		} else ++k;
	}

}

void Assistant::setiters(list<Line>::iterator* a, list<Line>::iterator* b, list<Line>::iterator* i, bool inc)
{
    if ((*i)->gets().find("{")!= string::npos)
    {
        ++(*i);
        *a=(*i);
        int k=1;
        while ( k>0)
        {
            if ((*i)->gets().find("{")!= string::npos)
                k++;
            else if ((*i)->gets().find("}")!= string::npos)
                k--;
            ++(*i);
        }
        --(*i);
        *b=(*i);
    }
    else
    {
        *a=(*i);
        ++(*i);
        *b=(*i);
        if (inc) --(*i);
    }
}

void Assistant::addinput(string str, string type, bool in)
{
	size_t found = str.find(type);
	if (found != string::npos)
	{
		string s = str.substr(found);
		bool pointer = false;
		if (type[ type.size() -1 ] == '*' )
			pointer = true;
		string t = type;
		if (pointer)
			t.erase( t.size() -2 );
		else
			t.erase( t.size() -1 );
		rtn->back()->Vars[s.substr(type.size()) ]->sett(t);
		rtn->back()->Vars[s.substr(type.size()) ]->setp(pointer);
		if (in) rtn->back()->Vars[s.substr(type.size()) ]->setuse(input);
	}
}

bool Assistant::checkdecls(string str, bool in)
{
	
		if ((str.find("int *")!=string::npos || str.find("char *")!=string::npos 
			|| str.find("signed int *")!=string::npos || str.find("signed char *")!=string::npos 
			|| str.find("unsigned int *")!=string::npos || str.find("unsigned char *")!=string::npos 
			|| str.find("long *")!=string::npos || str.find("short *")!=string::npos 
			|| str.find("signed long *")!=string::npos || str.find("signed short *")!=string::npos 
			|| str.find("unsigned long *")!=string::npos || str.find("unsigned short *")!=string::npos ) && str.find("sub_")==string::npos)
        {
			addinput( str, "unsigned int *", in);
			addinput( str, "signed int *", in);
			addinput( str, "int *", in);
			addinput( str, "unsigned long *", in);
			addinput( str, "signed long *", in);
			addinput( str, "long *", in);
			addinput( str, "unsigned char *", in);
			addinput( str, "signed char *", in);
			addinput( str, "char *", in);
			addinput( str, "unsigned short *", in);
			addinput( str, "signed short *", in);
			addinput( str, "short *", in);
			return true;
		}

		else if ((str.find("int ")!=string::npos || str.find("char ")!=string::npos 
			|| str.find("signed int")!=string::npos || str.find("signed char")!=string::npos 
			|| str.find("unsigned int")!=string::npos || str.find("unsigned char")!=string::npos 
			|| str.find("long")!=string::npos || str.find("short")!=string::npos 
			|| str.find("signed long")!=string::npos || str.find("signed short")!=string::npos 
			|| str.find("unsigned long")!=string::npos || str.find("unsigned short")!=string::npos ) && str.find("sub_")==string::npos)
        {
			addinput( str, "unsigned int ", in);
			addinput( str, "signed int ", in);
			addinput( str, "int ", in);
			addinput( str, "unsigned long ", in);
			addinput( str, "signed long ", in);
			addinput( str, "long ", in);
			addinput( str, "unsigned char ", in);
			addinput( str, "signed char ", in);
			addinput( str, "char ", in);
			addinput( str, "unsigned short ", in);
			addinput( str, "signed short ", in);
			addinput( str, "short ", in);
			return true;
		}
	else return false;
}


void Assistant::work(list<Line>::iterator beg, list<Line>::iterator en)
{
    list<Line>::iterator i;

    for(i = beg; i!=en; ++i)
    {

//----------------------------------IF---------------------------

        if  (i->gets().find("if ")!=string::npos)
        {
            string t, f, on;
            f=l.get();
            t=l.get();
            getcond(i->gets(), &on);
            generateif(on, t);
            ++i;
            list<Line>::iterator a, b;
            setiters(&a, &b, &i);
            ++i;
            if (i->gets().find("else")!= string::npos)
            {
                ++i;
                list<Line>::iterator c, d;
                setiters(&c, &d, &i, false);
                work(c, d);
                ++i;
            }
            generategoto(f);
            generatelabel(t);
            work(a, b);
            generatelabel(f);
            --i;
        }

//-----------------------------------FOR-------------------------------

        else if (i->gets().find("for ")!=string::npos)
        {
            string f1, f2, f3;
            string ex, co, lo;
            lo=l.get();
            co=l.get();
            ex=l.get();
            getfor(i->gets(), &f1, &f2, &f3);
            Parser p_f1(f1, *cnt, rtn->back()->Vars);
            p_f1.work();
            newblock();
            generatelabel(lo);
            generateif(f2, co);
            ++i;
            generategoto(ex);
            newblock();
            generatelabel(co);
            list<Line>::iterator a, b;
            setiters(&a, &b, &i),
            work(a, b);
            Parser p_f3(f3, *cnt, rtn->back()->Vars);
            p_f3.work();
            generategoto(lo);
            newblock();
            generatelabel(ex);
        }

//-------------------------------DO-WHILE---------------------------------------

        else if (i->gets().find("do ")!=string::npos)
        {
            string w;
            string on;
            on=l.get();
            newblock();
            generatelabel(on);
            ++i;
            list<Line>::iterator a, b;
            setiters(&a, &b, &i);
            work(a, b);
            ++i;
            getcond(i->gets(), &w);
            generateif(w, on);
        }

//-----------------------------------WHILE-------------------------------

        else if (i->gets().find("while")!=string::npos)
        {
            string w;
            string wh, co, ne;
            co=l.get();
            wh=l.get();
            ne=l.get();
            getcond(i->gets(), &w);
            newblock();
            generatelabel(wh);
            generateif(w, co);
            generategoto(ne);
            newblock();
            generatelabel(co);
            list<Line>::iterator a, b;
            ++i;
            setiters(&a, &b, &i);
            work(a, b);
            generategoto(wh);
            newblock();
            generatelabel(ne);
        }

//--------------------------------------LABELS------------------------

        else if (i->gets().find("goto")!=string::npos)
        {
            cnt->push_back( new  Line(i->gets()));
            newblock();
// TODO (András#1#): Here the original goto-s has to be dealt with,
//so if we see an original goto, we generate a goto from them in the new code. ...
//Same to the original labels;

        }

        else if (i->gets().find("LABEL_")!=string::npos)
        {
            cnt->push_back( new  Line(i->gets()));
            newblock();
        }


//----------------------------------FUNCTION -------------------------

        else if ((i->gets().find("int ")!=string::npos || i->gets().find("char ")!=string::npos || i->gets().find("void ")!=string::npos ) && i->gets().find("sub_")!=string::npos)
        {
            
			string tmp2 = i->gets().substr(i->gets().find("sub"), i->gets().size());
			
			stringstream ss(tmp2.substr(tmp2.find("(")+1, tmp2.find(")")-1));
			tmp2.erase( tmp2.find( "(" ) );
			
			rtn->push_back( new Function( l.getid(), tmp2 , l.getid()) );
			cnt = rtn->back()->back();

			string var;
			while ( getline(ss, var, ',') )
			{
				if (var[0] == ' ') var.erase( 0, 1);
				if (var[var.size()-1] == ')') var.erase( var.size() -1, 1);
				if (!var.empty())
					checkdecls(var, true);	
			}
            ++i;
            list<Line>::iterator a, b;
            setiters(&a, &b, &i);
            work( a, b);

        }

//----------------------------------DECLARATION -------------------------


		else if ( checkdecls(i->gets(), false ) )
		{
			list<Line>::iterator j=i;
			--i;
			lines->erase(j);
		}

//----------------------------------VARIOUS RETURNS AND CALLS -------------------------

		else if (i->gets().find("return")!=string::npos && i->gets().find("sub_")==string::npos)
        {
			string tmp = i->gets().substr( i->gets().find("return") + 7, i->gets().size() );
			rtn->back()->Vars[tmp]->setuse(output);
			cnt->push_back( new  Unc_jump("Return", -2, rtn->back()->Vars[tmp]));
			newblock();
        }
		
		else if (i->gets().find("return")!=string::npos && i->gets().find("sub_")!=string::npos)
        {
			stringstream s;
			string tmp = i->gets().substr( i->gets().find("return") + 7, i->gets().size() );
			tmp.erase( tmp.find("(") );
			s << tmp;
			stringstream ss (i->gets().substr( i->gets().find("(")+1 ) );
			string var;
			int par = 0;
			while ( getline(ss, var, ',') )
			{
				if (var[0] == ' ') var.erase( 0, 1);
				if (var[var.size()-1] == ')') var.erase( var.size() -1, 1);
				if (!var.empty())
				{

					cnt->push_back( new  Call( s.str(), rtn->back()->Vars[var], true ));
					par++;
				}
				
			}
			
			s << " " << par;
			cnt->push_back( new  Call( s.str(), rtn ));
			cnt->push_back( new Call( s.str(), rtn->back()->Vars[tmp], false, true));
			cnt->push_back( new  Unc_jump("Return", -2, rtn->back()->Vars[tmp]));
			rtn->back()->Vars[tmp]->setuse(output);
			newblock();
        }
		
        else if (i->gets().find("sub_")!=string::npos && i->gets().find("=")==string::npos)
        {
            stringstream s;
			string tmp(i->gets().substr( i->gets().find("sub_") ));
			tmp.erase( tmp.find("(") );
			s << tmp;
			stringstream ss (i->gets().substr( i->gets().find("(")+1 ) );

			string var;
			int par = 0;
			while ( getline(ss, var, ',') )
			{
				if (var[0] == ' ') var.erase( 0, 1);
				if (var[var.size()-1] == ')') var.erase( var.size() -1, 1);
				if (!var.empty())
				{

					cnt->push_back( new  Call( s.str(), rtn->back()->Vars[var], true ));
					par++;
				}
				
			}
			
			s << " " << par;
			cnt->push_back( new  Call( s.str(), rtn ));

        }
		else if (i->gets().find("sub_")!=string::npos && i->gets().find("=")!=string::npos)
        {
            stringstream s;
			string tmp(i->gets().substr( i->gets().find("sub_") ));
			tmp.erase( tmp.find("(") );
			s << tmp;
			string retr = i->gets();
			retr.erase( retr.find(" "), retr.size() );
			stringstream ss (i->gets().substr( i->gets().find("(")+1 ) );

			string var;
			int par=0;
			while ( getline(ss, var, ',') )
			{
				if (var[0] == ' ') var.erase( 0, 1);
				if (var[var.size()-1] == ')') var.erase( var.size() -1, 1);
				if (!var.empty())
				{

					cnt->push_back( new  Call( s.str(), rtn->back()->Vars[var], true ));
					par++;
				}
			}
			s << " " << par;
			cnt->push_back( new  Call( s.str(), rtn ));
			cnt->push_back( new Call( s.str(), rtn->back()->Vars[retr], false, true));
        }
//----------------------------------EQU-LIKE----------------------------

        else if (i->gets().find("=")!=string::npos || i->gets().find("++")!=string::npos || i->gets().find("--")!=string::npos)
        {
            Parser p(i->gets(), *cnt, rtn->back()->Vars);
            p.work();
        }

        else if (i->gets() == "")
        {

        }

        else
        {
            cnt->push_back( new  Line(i->gets()));
        }
    }

    rtn->cleanup();
}

void Assistant::setfunctions()
{
	rtn->setfunctions();
}

#include "assistant.h"

#include <cstdlib>
#include <stdio.h>

void Assistant::getfor(string from, string *s1, string *s2, string *s3)
{
    stringstream ss( from.substr(from.find("(")+2, from.size()) );
    getline(ss, *s1, ';');
    getline(ss, *s2, ';');
    if (*s2 == " ") *s2 = "1";
    getline(ss, *s3, ')');
    s3->erase(s3->size()-1, 1);

    if (((s2->find("=")!= string::npos) || (s2->find(">")!= string::npos)
                  || (s2->find("<")!= string::npos)) && (*s2 != "1"))
    {
        Parser p(*s2, *cnt, bl->Vars);
        p.work();
        *s2 = p.str();
    }
}

void Assistant::getwhile(string from, string *s1)
{
    string tmp = from.substr(from.find("(")+2, from.size());
    tmp.erase(tmp.size()-1, 1);
    tmp.erase(tmp.size()-1, 1);
    stringstream ss(tmp);
    getline(ss, *s1, ';');

    if ((from.find("=")!= string::npos) || (from.find(">")!= string::npos) || (from.find("<")!= string::npos))
    {
        Parser p(*s1, *cnt, bl->Vars);
        p.work();
        *s1 = p.str();
    }
}

void Assistant::getif(string from, string *s1)
{
    string tmp = from.substr( from.find("if") +4, from.size() ) ;
    tmp.erase(tmp.size()-1, 1);
    tmp.erase(tmp.size()-1, 1);
    stringstream ss(tmp);
    getline(ss, *s1, ';');

    if ((from.find("=")!= string::npos) || (from.find(">")!= string::npos) || (from.find("<")!= string::npos))
    {
        Parser p(*s1, *cnt, bl->Vars);
        p.work();
        *s1 = p.str();
    }
}

void Assistant::newblock()
{
    bl->id_deal++;
    bl->blocks.push_back(new CInstructionsContainer(bl->id_deal));
    cnt = bl->blocks.back();
}

void Assistant::generateif(string deal, string label)
{
    size_t found = deal.find("&&");
    size_t found2= deal.find("||");

    cout << found << ", " << found2 << endl;

    if (found == string::npos)
    {
        if (found2 == string::npos)
        {
            if (((deal.find("=")!= string::npos) || (deal.find(">")!= string::npos)
                      || (deal.find("<")!= string::npos)))
                {
                    Parser p_if(deal, *cnt, bl->Vars);
                    p_if.work(false);
                    deal = p_if.str();
                }
            string tmp = label;
            int i=0;
            tmp.erase(0, 6);
            stringstream ss(tmp);
            ss >> i;
            label.erase(6, label.size());
            cnt->push_back( new Cond_jump(deal, i) );

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
    for (list<Line>::iterator i = lines->begin(); i != lines->end(); ++i)
    {
        if ( i->gets().find("//") != string::npos )
            i->ersmid( i->gets().find("//"), i->gets().size() );
    }

    for (list<Line>::iterator i = lines->begin(); i != lines->end(); ++i)
    {
        size_t found = i->gets().find("*(_DWORD *)");
        while (found != string::npos)
        {
            i->ersmid(found, 11);
            found = i->gets().find("*(_DWORD *)");
        }
    }

    for (list<Line>::iterator i = lines->begin(); i != lines->end(); ++i)
    {
        size_t found2 = i->gets().find("sub_");
        size_t found = i->gets().find("signed int ");
        if (found != string::npos && found2 == string::npos)
        {
            string s = i->gets().substr(found), t = "signed int";
            s.erase( s.find(";"), s.size() );
#ifdef DEBUG2
            cout << bl->Vars[s.substr(4) ]->getname() << endl;
#endif

            bl->Vars[s.substr(11) ]->sett("signed int");
            list<Line>::iterator j=i;
            --i;
            lines->erase(j);
        }
        /*found = i->gets().find("int ");
        if (found != string::npos && found2 == string::npos)
        {
            string s = i->gets().substr(found), t = "int";
			size_t exists = s.find(";");
			if ( exists != string::npos )
				s.erase( exists, s.size() );
#ifdef DEBUG2
            cout << bl->Vars[s.substr(4) ]->getname() << endl;
#endif

            bl->Vars[s.substr(4) ]->sett("int");
            list<Line>::iterator j=i;
            --i;
            lines->erase(j);
        }*/
        found = i->gets().find("char ");
        if (found != string::npos && found2 == string::npos)
        {
            string s = i->gets().substr(found);
            s.erase( s.find(";"), s.size() );
#ifdef DEBUG2
            bl->Vars[s.substr(5) ]->sett("char");
#endif
            list<Line>::iterator j=i;
            --i;
            lines->erase(j);
        }
    }

    // (*(_BYTE *)  is...?

    for (list<Line>::iterator i = lines->begin(); i != lines->end(); ++i)
    {
        size_t found = i->gets().find(" = -");
        if (found != string::npos)
            i->repl(found);
        while((i->gets())[0] == ' ') i->ersbeg();
        if((i->gets())[i->gets().size()-1] == ';') i->ersend();
//
//        size_t found1 = i->gets().find("(");
//
//        while (found1 != string::npos)
//        {
//            cout << found1 << endl;
//            if ( i->gets()[found1 + 1] != ' ')
//                i->ins(found1 + 1);
//            found1 = i->gets().find("(", found1+1);
//        }
//
//        size_t found2 = i->gets().find(")");
//
//        while ( found2 != string::npos)
//        {
//            if ( i->gets()[found2 - 1] != ' ')
//                i->ins(found2);
//            found2 = i->gets().find(")", found2+1, i->gets().size());
//        }

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
            getif(i->gets(), &on);
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

        else if (i->gets().find("for")!=string::npos)
        {
            string f1, f2, f3;
            string ex, co, lo;
            lo=l.get();
            co=l.get();
            ex=l.get();
            getfor(i->gets(), &f1, &f2, &f3);
            Parser p_f1(f1, *cnt, bl->Vars);
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
            Parser p_f3(f3, *cnt, bl->Vars);
            p_f3.work();
            generategoto(lo);
            newblock();
            generatelabel(ex);
        }

//-------------------------------DO-WHILE---------------------------------------

        else if (i->gets().find("do")!=string::npos)
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
            getwhile(i->gets(), &w);
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
            getwhile(i->gets(), &w);
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

//----------------------------------EQU-LIKE----------------------------

        else if (i->gets().find("=")!=string::npos || i->gets().find("++")!=string::npos || i->gets().find("--")!=string::npos)
        {
            Parser p(i->gets(), *cnt, bl->Vars);
            p.work();
            //cnt->push_back( new  CThreeAdressInstruction(i->gets()));
        }

//----------------------------------OTHER-STUFF-------------------------

        else if ((i->gets().find("int ")!=string::npos ||
                 i->gets().find("char ")!=string::npos ||
                 i->gets().find("void ")!=string::npos ) && i->gets().find("sub_")!=string::npos)
        {
            
			string tmp2 = i->gets().substr(i->gets().find("sub"), i->gets().size());
            string tmp = "		<Function Name=\"" + tmp2 + "\">";

			newblock();
            
            ++i;
            list<Line>::iterator a, b;
            setiters(&a, &b, &i);
            work( a, b);
            tmp = "		</Function><!-- " + tmp2 + " -->";
            generatelabel(tmp);

            newblock();
        }

        else if (i->gets().find("sub_")!=string::npos)
        {
            cnt->push_back( new  Call(i->gets()));
        }

		else if (i->gets().find("return")!=string::npos)
        {
            cnt->push_back( new  Line("Return"));
        }
		//Test comment.
		//Test comment 2

        else if (i->gets() == "")
        {

        }

        else
        {
            cnt->push_back( new  Line(i->gets()));
        }
    }

    bl->cleanup();
}



#define PSEUDOCODE

#if !PSEUDOCODE

void recalc (Variable con)
{
    /*
     * We have to replace all the occurrences of the constant value,
     * so we should have a list of all the instructions using it.
     * 
     * search_for_referers() - gives a list of the instructions that are
     *                         using the variable with the given ID
     */
    list<Instructios> inst = search_for_referers(con.id);

    foreach (Instruction i in inst)
    {
        /*
         * calc_ins() - from the given constant and intruction it generates
         *              a set of instructions with the same result, but
         *              with dinamically calculated constant.
         *              it gives us a list of the altered instructions,
         *              and a list of the new (temporary, local) variables used.
         */
        list<Instructions> altered;
        list<Variables> newvars;
        calc_ins(con, i, altered, newvars);

        /*
         * We have to insert the new set of instructions to the place of
         * the original instruction, and we have to insert the new variables
         * to the variable list of the function.
         */
         i.basicblock().insert_to_the_proper_place (altered);
         i.function().insert_to_the_proper_place (newvars);
    }
}

void calc_ins (Variable con, Instruction ins, List<Instruction> altered, List<Variable> newvars)
{
    
}

#endif
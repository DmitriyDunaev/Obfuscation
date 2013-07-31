#define PSEUDOCODE

#if !PSEUDOCODE

void doit (Function func)
{
    /*
     * As we have discussed, we start from the point called
     * "fake exit block", the one and only ultimate endpoint
     * of all functions ever created. Thank you Kohlmann!
     *
     * getlastblock() - it is supposed to give the function's "fake exit block"
     */
    BasicBlock lastblock = func.getlastblock();

    /*
     * At first we go through all the instructions and deal with all their
     * referenced variables. At the and of this we will have the list of the alive
     * variables for all instructions.
     */
    recursive(lastblock, true);

    /*
     * We want to go through the basic blocks again, so we have to reset
     * all the marked_as_done flags.
     */
    reset(lastblock);

    /*
     * Now we have the list of alive variables, we have to make the list
     * of dead variables.
     * We can use the same recursive function to do this for all the instructions.
     */
    recursive(lastblock, false);
}

void recursive (BasicBlock actual, bool looking_for_alives)
{
    if (looking_for_alives)
    {
        /*
         * We deal with every instruction in the basic block,
         * and in every instruction we deal with every referenced variable.
         * The referenced variables should be marked as alive in all the instructions
         * accesible from here.
         *
         * deal_with_var() - it will be described in detail in the forthcoming parts
         */
        foreach (Instruction ins in actual.instructions)
	    {
		     foreach (Variable var in ins.RefVars)
	         {
		        deal_with_var(var, ins);
	         }
	    }
    }

    else
    {
        /*
         * Because a variable can be dead or alive and nothing else, we can get
         * the list of dead variables by taking away the list of alive variables
         * from the list of all variables.
         * And we must do this for all the instructions.
         * 
         * list_of_variables - the list of all variables present in the function
         * dead - list of dead variables at the point of the instruction
         */
         foreach (Instruction ins in actual.instructions)
            ins.dead = list_of_variables - ins.alive;
    }

    /*
     * mark_as_done() - we have finished the task with this basic block,
     *                  and we should not come back here anymore
     */
    actual.mark_as_done();

    /*
     * Now that this basic block is finished we should do the same things
     * with its predecessors recursively.
     * We only should deal with the basic blocks not marked as done.
     *
     * is_it_done_yet() - tells us whether that block is marked as done or not 
     */
    foreach (BasicBlock block in actual.predecessors)
    {
        if ( !block.is_it_done_yet() )
            recursive (block, looking_for_alives);
    }
}

void reset(BasicBlock actual)
{
     /* mark_as_undone() - reset the marked_as_done flag in the basic block */
    actual.mark_as_undone();
    
    foreach (BasicBlock block in actual.predecessors)
    {
        if ( block.is_it_done_yet() )
            reset (block);
    }
}

void deal_with_var (Variable var, Instruction ins)
{
    /* 
     * This variable is used somewhere after this instruction, so it is alive here.
     * 
     * alives - it contains the alive variables at the point of the given instruction
     * add() - adds an element to the list(?) of the alive variables
     */
    ins.alives.add(var);
    
    /*
     * next() - gives a list of the instructions followed by the actual instruction
     *          - if we are in the middle of the basic block, then it consists of only one instruction
     *          - if we are on the beginning of the basic block then it is the list of all
     *            the predecessing basic block's last instruction         
     */
    List<Instruction> next = ins.next();

    /*
     * Now we have that list of instructions, we should do the same thing we have done
     * to this instruction, assuming that it had not been done already.
     */
    foreach (Instruction i in next)
    {
        /*
         * If the variable is in the instruction's alives list, then it indicates
         * that we have dealt with this instruction.
         * 
         * has_it(var) - tells us whether 'var' is in the list or not 
         */
        if ( !i.alives.has_it(var) )
            deal_with_var(var, i);  
    }
}


#endif
#define PSEUDOCODE

/*
 * TODO:
 * 
 * - Function: getlastblock() - gives reference to the functions "fake exit block"
 *   (see in the doit() function)
 * 
 * - BasicBlock: ID - could we make it public, or some getter method for it?
 *                
 * - Instruction: dead - a list of the dead variables at the point of that instruction
 *                     - as the algorithm starts we need it to be filled with all the
 *                       variables present in the function
 *     DONE:      dead.contains() - this should be a function of the List<Variables>, and it should
 *                                  return a boolean value depending on the presence of the given
 *                                  variable in the list
 *                                  if the List already has a function like this, then it's OK,
 *                                  if not, then the Instruction could have something like this...
 *     DONE:      dead.remove() - also a function the List should have, it removes a given 
 *                                variable from the list (in C++, it exists and works like this)
 *                                
 * - previous() : returns a list of all the instructions that are followed by
 *                this instruction
 *  (see in the deal_with_vars() function)
 */

#if !PSEUDOCODE

class DeadAlgorithm
{
    /* 
     * To make sure we don't deal with a basic block twice, we save the ID
     * of the basic blocks we have been to into a list.
     * We could make this an attribute for the DeadAlgorithm class.
     */
    private List<IDManager> done_ids;

    /*
     * Before the algorithm starts we assume that all instructions have a
     * list of dead variables which is filled with all the variabes present in
     * the function. During the algorithm we will remove the variables from these
     * lists that are not really dead at the given point.
     */
    public void doit(Function func)
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
         * We go through all the instructions and deal with all their
         * referenced variables. At the and of this we will have the list of the dead
         * variables for all instructions.
         */
        recursive(lastblock);
    }

    private void recursive(BasicBlock actual)
    {
        /*
         * We deal with every instruction in the basic block,
         * and in every instruction we deal with every referenced variable.
         * The referenced variables should be removed from the dead variables list
         * in all the instructions accesible from here.
         *
         * deal_with_var() - it will be described in detail in the forthcoming parts
         */
        foreach (Instruction ins in actual.Instructions)
        {
            foreach (Variable var in ins.RefVariables)
                deal_with_var(var, ins);
        }

        /*
         * We have finished the task with this basic block, and we should not
         * come back here anymore, so we save its ID into the done_ids list.
         */
        done_ids.add(actual.ID);

        /*
         * Now that this basic block is finished we should do the same things
         * with its predecessors recursively.
         * We only should deal with the basic blocks not marked as done.
         */
        foreach (BasicBlock block in actual.Predecessors)
        {
            if (!done_ids.contains(block.ID))
                recursive(block);
        }
    }

    private void deal_with_var(Variable var, Instruction ins)
    {
        /* 
         * This variable is used somewhere after this instruction, so it is alive here.
         * 
         * dead - it contains the dead variables at the point of the given instruction
         * remove() - remove an element from the list(?) of the dead variables
         */
        ins.dead.remove(var);

        /*
         * previous() - gives a list of the instructions followed by the actual instruction
         *            - if we are in the middle of the basic block, then it consists of only one instruction
         *            - if we are at the beginning of the basic block then it is the list of all
         *              the predecessing basic block's last instruction
         *            - if we are at the beginning of the first basic block (the one with no predecessors)
         *              then it is an empty list, meaning that we have nothing left to do here
         */
        List<Instruction> previous = previous(ins);

        /*
         * Now we have that list of instructions, we should do the same thing we have done
         * to this instruction, assuming that it had not been done already.
         */
        foreach (Instruction i in previous)
        {
            /*
             * If the variable is not in the instruction's dead variables list, then it indicates
             * that we have dealt with this instruction.
             * 
             * contains(var) - tells us whether 'var' is in the list or not 
             */
            if (i.dead.contains(var))
                deal_with_var(var, i);
        }
    }
}

#endif

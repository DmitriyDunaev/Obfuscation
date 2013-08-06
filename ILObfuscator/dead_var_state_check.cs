#define PSEUDOCODE

#if !PSEUDOCODE

/*
 * Wrapper function over the recursive algorithm.
 * I think we cannot avoid to run through the instructions twice, because
 * I fear if we change states before we assure it is valid everywhere,
 * we may end up in an unconsistent state, where in some places the state has been
 * already changed, and at other places it has not.
 */
bool refresh_states (Variable var, Variable.State newstate, Instruction ins)
{
    if ( check_states(var, newstate, ins) )
    {
        change_states(var, newstate, ins);
        return true;
    }    
    else
        return false;
}

/*
 * Recursive function to check if the required change of state is valid or not.
 * First it checks it in the instruction, and if it is not valid, then returns false,
 * if the states are the same, then it stops (returning true value), and if the state
 * can be overwritten, then it continues with the next instruction(s).
 */
bool check_states (Variable var, Variable.State newstate, Instruction ins)
{
    switch ( collide (newstate, ins.DeadVariables[var]) )
    {
        case -1:
            return false;

        case 0:
            return true;

        case 1:
            List<Instruction> next = GetNextInstructions(ins);
            foreach (Instruction i in next)
	        {
		        if ( !check_states(var, newstate, i) )
                    return false;
	        }
            return true;
    }
}

int collide (Variable.State newstate, Variable.State oldstate)
{
    
}

#endif
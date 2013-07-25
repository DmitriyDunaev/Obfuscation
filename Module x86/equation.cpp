#include "equation.h"


Equation::Equation (COperand** arr, bool neg) : neg(neg)
{
    for (int i = 0; i < 2; i++)
        ops[i] = arr[i];
}

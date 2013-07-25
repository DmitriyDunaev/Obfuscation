#include "quadruple.h"

using namespace std;

Quadruple::Quadruple(string op, COperand** arr)
{
    oprtr = op;
    for (int i = 0; i < 3; i++)
        ops[i] = arr[i];
}

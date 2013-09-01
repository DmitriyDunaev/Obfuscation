using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Internal;


namespace Platform_x86
{
    public static class Assembler
    {

        public static string GetAssemblyFromTAC(Function func)
        {
            StringBuilder sb = new StringBuilder();

            Obfuscator.Traversal.ReorderBasicBlocks(func);


            return sb.ToString();
        }

    }
}

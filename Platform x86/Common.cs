using Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Platform_x86
{
    public static class Common
    {

        /// <summary>
        /// Whether to fix random VALID parameters or leave it to the user (Used for testing)
        /// true: select the values and write them fixed to a generated EXE file
        /// false: generate fake input parameters, later provided by the user
        /// </summary>
        public static bool FixFakeParameters = true;
    
    }
}

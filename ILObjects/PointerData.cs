using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Objects
{
    /// <summary>
    /// Container class for a pointer.
    /// </summary>
    [Serializable]
    public class PointerData
    {
        // Attributes

        /// <summary>
        /// The pointed variable.
        /// </summary>
        public Variable PointsTo { get; set; }

        /// <summary>
        /// The state.
        /// </summary>
        public Variable.State State { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="state">The state of the pointer.</param>
        /// <param name="var">The variable it points to.</param>
        public PointerData(Variable.State state, Variable var)
        {
            State = state;
            PointsTo = var;
        }
    }
}

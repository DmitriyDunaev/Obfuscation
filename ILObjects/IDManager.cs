using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Objects
{
    /// <summary>
    /// Class for generating and storing the Unique Identifiers for objects 
    /// </summary>
    [Serializable]
    public class IDManager
    {
        private string ID;
        private const string startID = "ID_";

        public IDManager()
        {
            ID = string.Concat(startID, Guid.NewGuid().ToString()).ToUpper();
        }

        public IDManager(string id)
        {
            ID = id;
        }

        public override bool Equals(object obj)
        {
            return (obj as IDManager) == null ? base.Equals(obj) : ((IDManager)obj).ID == ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return ID;
        }
    }
}

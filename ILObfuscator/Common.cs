using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obfuscator
{
    public static class Common
    {

        public static object DeepClone(object obj)
        {
            object objResult = null;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bf.Serialize(ms, obj);

                ms.Position = 0;
                objResult = bf.Deserialize(ms);
            }
            return objResult;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UninterruptedPlayExp
{
    public class CloneableDictionary<TKey , TValue>  : Dictionary<TKey, TValue>,ICloneable where TValue : ICloneable 
    {

        public object Clone()
        {
            CloneableDictionary<TKey, TValue> clone = new CloneableDictionary<TKey, TValue>();
                foreach (KeyValuePair<TKey, TValue> pair in this)
                {
                    clone.Add(pair.Key,(TValue)pair.Value.Clone());
                }
            
            
            return clone                    ;
        }
    }
}
